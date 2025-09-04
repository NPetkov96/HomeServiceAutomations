
using DataLayer;
using Microsoft.OpenApi.Models;

namespace HomeApi
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddScoped<DataBaseContext>();

            builder.Services.AddControllers();
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
    }
}
