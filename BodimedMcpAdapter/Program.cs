using BodimedMcpAdapter.Configuration;
using BodimedMcpAdapter.Mcp;
using BodimedMcpAdapter.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.IdentityModel.Tokens;
using ModelContextProtocol.AspNetCore;
using ModelContextProtocol.AspNetCore.Authentication;

namespace BodimedMcpAdapter;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        var bodimedApi = BodimedApiOptions.FromConfiguration(
            builder.Configuration,
            requireHttps: builder.Environment.IsProduction());
        var mcpAuth = McpAuthOptions.FromConfiguration(
            builder.Configuration,
            requireHttps: builder.Environment.IsProduction());
        var mcpResource = new Uri(mcpAuth.PublicBaseUri, "mcp");

        builder.Services.Configure<ForwardedHeadersOptions>(options =>
        {
            options.ForwardedHeaders = ForwardedHeaders.XForwardedProto;
            options.KnownNetworks.Clear();
            options.KnownProxies.Clear();
        });

        builder.Logging.AddFilter("System.Net.Http.HttpClient", LogLevel.Warning);
        builder.Logging.AddFilter("ModelContextProtocol", LogLevel.Warning);
        builder.Services.AddSingleton(bodimedApi);
        builder.Services.AddHttpClient<IBodimedApiClient, BodimedApiClient>(client =>
        {
            client.BaseAddress = bodimedApi.BaseUri;
            client.Timeout = TimeSpan.FromSeconds(30);
        }).ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
        {
            AllowAutoRedirect = false
        });

        builder.Services
            .AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = McpAuthenticationDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.Authority = mcpAuth.Issuer.ToString().TrimEnd('/');
                options.RequireHttpsMetadata = builder.Environment.IsProduction();
                options.MapInboundClaims = false;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    ValidateIssuer = true,
                    // Auth0 issuer identifiers include the trailing slash and JWT issuer
                    // validation is an exact string comparison.
                    ValidIssuer = mcpAuth.Issuer.ToString(),
                    ValidateAudience = true,
                    ValidAudience = mcpAuth.Audience,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.FromMinutes(2)
                };
            })
            .AddMcp(options =>
            {
                options.ResourceMetadata = new()
                {
                    Resource = mcpResource.ToString(),
                    AuthorizationServers =
                    {
                        mcpAuth.AuthorizationServer.ToString()
                    },
                    ScopesSupported = { mcpAuth.RequiredScope }
                };
            });

        builder.Services.AddAuthorization(options =>
        {
            options.AddPolicy(McpPolicies.Endpoint, policy =>
                policy.RequireAuthenticatedUser());
            options.AddPolicy(McpPolicies.Write, policy =>
                policy.RequireAuthenticatedUser().RequireAssertion(context =>
                    HasScope(context.User, mcpAuth.RequiredScope)));
        });

        builder.Services
            .AddMcpServer(options => options.ServerInstructions =
                "create_patient is a consequential write operation. Show the user every exact patient field and blood test, and obtain explicit confirmation before calling it with confirmed=true.")
            .WithHttpTransport(options =>
                options.SessionMode = HttpServerSessionMode.Stateless)
            .AddAuthorizationFilters()
            .WithTools<BodimedMcpTools>();

        var app = builder.Build();

        app.UseForwardedHeaders();
        app.UseAuthentication();
        app.UseAuthorization();
        app.MapGet("/health", () => Results.Ok(new { status = "healthy" }))
            .AllowAnonymous();
        app.MapMcp("/mcp").RequireAuthorization(McpPolicies.Endpoint);

        app.Run();
    }

    private static bool HasScope(
        System.Security.Claims.ClaimsPrincipal user,
        string requiredScope)
    {
        return user.FindAll("scope")
            .Concat(user.FindAll("scp"))
            .SelectMany(claim => claim.Value.Split(
                ' ',
                StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
            .Contains(requiredScope, StringComparer.Ordinal);
    }
}
