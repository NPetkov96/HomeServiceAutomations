
using DataLayer;
using Extensions;
using Microsoft.OpenApi.Models;

namespace HomeApi
{
    public class Program
    {
        public static void Main(string[] args)
        {
            try
            {

                var builder = WebApplication.CreateBuilder(args);

                builder.Services.AddScoped<DataBaseContext>();

                builder.Services.AddControllers()
                    .AddJsonOptions(options =>
                    {
                        options.JsonSerializerOptions.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.Never;
                    });

                builder.Services.AddOpenApi();

                builder.Services.AddCors(opt =>
                opt.AddDefaultPolicy(p =>
                {
                    p.AllowAnyOrigin();
                    p.AllowAnyMethod();
                    p.AllowAnyHeader();
                }));

                builder.Services.AddSignalR();
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

                app.UseSwagger();
                app.UseSwaggerUI();
                app.UseCors();
                app.UseHttpsRedirection();
                app.UseAuthorization();
                app.MapControllers();

                app.Run();
            }
            catch (Exception ex)
            {
                WriteLog.Log(ex.Message, ex.StackTrace!);
            }
        }
    }
}
