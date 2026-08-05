
using DataLayer;
using Extensions;
using Microsoft.OpenApi.Models;
using System.Security.Cryptography;
using System.Text;

namespace HomeApi
{
    public class Program
    {
        public static void Main(string[] args)
        {
            try
            {

                var builder = WebApplication.CreateBuilder(args);

                builder.Services.AddDbContext<DataBaseContext>();

                builder.Services.AddControllers()
                    .AddJsonOptions(options =>
                    {
                        options.JsonSerializerOptions.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.Never;
                    });

                builder.Services.AddOpenApi();

                builder.Services.AddSignalR();
                builder.Services.AddHealthChecks();
                builder.Services.AddEndpointsApiExplorer();
                builder.Services.AddSwaggerGen(o =>
                {
                    o.SwaggerDoc("v1", new OpenApiInfo
                    {
                        Version = "v1.0.7",
                        Title = "API",
                        Description = ""
                    });
                }
                    );
                var app = builder.Build();

                var requireApiKey = builder.Configuration.GetValue<bool>("ApiSecurity:RequireApiKey");
                var configuredApiKey = builder.Configuration["ApiSecurity:ApiKey"];
                if (requireApiKey && string.IsNullOrWhiteSpace(configuredApiKey))
                {
                    throw new InvalidOperationException("API key protection is required, but no API key is configured.");
                }

                if (app.Environment.IsDevelopment())
                {
                    app.UseSwagger();
                    app.UseSwaggerUI();
                    app.UseHttpsRedirection();
                }

                if (requireApiKey)
                {
                    app.Use(async (context, next) =>
                    {
                        if (context.Request.Path.Equals("/health", StringComparison.OrdinalIgnoreCase))
                        {
                            await next();
                            return;
                        }

                        var providedApiKey = context.Request.Headers["X-Api-Key"].ToString();
                        var expectedHash = SHA256.HashData(Encoding.UTF8.GetBytes(configuredApiKey!));
                        var providedHash = SHA256.HashData(Encoding.UTF8.GetBytes(providedApiKey));

                        if (!CryptographicOperations.FixedTimeEquals(expectedHash, providedHash))
                        {
                            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                            await context.Response.WriteAsJsonAsync(new { error = "Unauthorized" });
                            return;
                        }

                        await next();
                    });
                }

                app.UseAuthorization();
                app.MapControllers();
                app.MapHealthChecks("/health");

                app.Run();
            }
            catch (Exception ex)
            {
                WriteLog.Log(ex.Message, ex.StackTrace!);
            }
        }
    }
}
