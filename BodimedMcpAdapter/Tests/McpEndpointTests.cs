using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Http.Json;
using System.Security.Claims;

namespace BodimedMcpAdapter.Tests;

[CollectionDefinition(Name, DisableParallelization = true)]
public sealed class McpEndpointCollection
{
    public const string Name = "MCP endpoint environment";
}

[Collection(McpEndpointCollection.Name)]
public sealed class McpEndpointTests(McpAdapterFactory factory)
    : IClassFixture<McpAdapterFactory>
{
    [Fact]
    public async Task Health_IsAvailableWithoutAuthentication()
    {
        using var client = factory.CreateClient();

        using var response = await client.GetAsync("/health");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Mcp_RejectsAnUnauthenticatedRequestWithBearerChallenge()
    {
        using var client = factory.CreateClient();
        using var request = new HttpRequestMessage(HttpMethod.Post, "/mcp")
        {
            Content = JsonContent.Create(new { })
        };
        request.Headers.Accept.ParseAdd("application/json");
        request.Headers.Accept.ParseAdd("text/event-stream");
        request.Headers.TryAddWithoutValidation("X-Forwarded-Proto", "https");

        using var response = await client.SendAsync(request);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        var challenge = Assert.Single(response.Headers.WwwAuthenticate);
        Assert.Equal("Bearer", challenge.Scheme);
        Assert.Contains("resource_metadata=", challenge.Parameter);
        Assert.Contains("https://localhost/", challenge.Parameter);
    }

    [Fact]
    public async Task ProtectedResourceMetadata_AdvertisesWriteScope()
    {
        using var client = factory.CreateClient();

        using var response = await client.GetAsync(
            "/.well-known/oauth-protected-resource/mcp");
        var content = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("bodimed.write", content);
        Assert.Contains("http://authorization.test", content);
    }

    [Fact]
    public void JwtValidation_PreservesTheAuth0StyleTrailingSlashInIssuer()
    {
        var options = factory.Services
            .GetRequiredService<IOptionsMonitor<JwtBearerOptions>>()
            .Get(JwtBearerDefaults.AuthenticationScheme);

        Assert.Equal(
            "http://issuer.test/",
            options.TokenValidationParameters.ValidIssuer);
    }

    [Fact]
    public async Task TokenWithoutWriteScope_DoesNotSatisfyToolPolicy()
    {
        var authorization = factory.Services.GetRequiredService<IAuthorizationService>();
        var user = PrincipalWithScope("other.scope");

        var result = await authorization.AuthorizeAsync(
            user,
            resource: null,
            Mcp.McpPolicies.Write);

        Assert.False(result.Succeeded);
    }

    [Fact]
    public async Task TokenWithWriteScope_SatisfiesToolPolicy()
    {
        var authorization = factory.Services.GetRequiredService<IAuthorizationService>();
        var user = PrincipalWithScope("bodimed.write");

        var result = await authorization.AuthorizeAsync(
            user,
            resource: null,
            Mcp.McpPolicies.Write);

        Assert.True(result.Succeeded);
    }

    private static ClaimsPrincipal PrincipalWithScope(string scope)
    {
        return new ClaimsPrincipal(new ClaimsIdentity(
            [new Claim("sub", "test-user"), new Claim("scope", scope)],
            authenticationType: "test"));
    }
}

public sealed class McpAdapterFactory : WebApplicationFactory<Program>
{
    private static readonly IReadOnlyDictionary<string, string> TestEnvironment =
        new Dictionary<string, string>
        {
            ["ASPNETCORE_ENVIRONMENT"] = "Development",
            ["BODIMED_API_KEY"] = "integration-test-key",
            ["MCP_AUTH_ISSUER"] = "http://issuer.test",
            ["MCP_AUTH_AUDIENCE"] = "bodimed-mcp",
            ["MCP_AUTH_AUTHORIZATION_SERVER"] = "http://authorization.test",
            ["MCP_AUTH_PUBLIC_BASE_URL"] = "http://localhost",
            ["MCP_AUTH_REQUIRED_SCOPE"] = "bodimed.write"
        };

    private readonly Dictionary<string, string?> _previousEnvironment = [];

    public McpAdapterFactory()
    {
        foreach (var setting in TestEnvironment)
        {
            _previousEnvironment[setting.Key] =
                Environment.GetEnvironmentVariable(setting.Key);
            Environment.SetEnvironmentVariable(setting.Key, setting.Value);
        }
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);

        foreach (var setting in _previousEnvironment)
        {
            Environment.SetEnvironmentVariable(setting.Key, setting.Value);
        }
    }
}
