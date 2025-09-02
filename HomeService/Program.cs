
using HomeService.Services;
using Microsoft.OpenApi.Models;
using System.Reflection;

namespace HomeService
{
    public class Program
    {
        public static void Main(string[] args)
        {
            try
            {


                var builder = WebApplication.CreateBuilder(args);
                Configuration.Appsettings = builder.Configuration;
                Configuration.ConnectionString = builder.Configuration.GetConnectionString("ConnectionString");

                builder.Services.AddControllers();
                builder.Services.AddCors(opt =>
                opt.AddDefaultPolicy(p =>
                {
                    p.AllowAnyOrigin();
                    p.AllowAnyMethod();
                    p.AllowAnyHeader();
                }));

                builder.Services.AddSignalR();
                builder.Services.AddEndpointsApiExplorer();
                builder.Services.AddOpenApi();
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

                IHostBuilder hostBuilder = null;

                if (Configuration.EnableServices)
                {
                    if (!Configuration.EnableAPI)
                    {
                        hostBuilder = Host.CreateDefaultBuilder(args).ConfigureServices((hostContext, services) =>
                        {
                            services.AddScoped<Operations.NoniCampaignsAttachemnts.CampaignsOperations>();
                            services.AddScoped<Operations.NoniCampaignsAttachemnts.EmailReader>();
                            services.AddScoped<Operations.NoniCampaignsAttachemnts.CreateCampaign>();
                            services.AddScoped<Operations.NoniCampaignsAttachemnts.ExtractPropertiesByEmail>();


                            var hostedService = typeof(ScheduledTask);
                            var assembly = Assembly.GetExecutingAssembly();

                            var hostedServices = assembly.GetTypes()
                            .Where(t => hostedService.IsAssignableFrom(t) && t.IsClass && !t.IsAbstract && !t.IsInterface).ToList();

                            var method = typeof(ServiceCollectionHostedServiceExtensions)
                            .GetMethods(BindingFlags.Static | BindingFlags.Public)
                            .FirstOrDefault(m => m.Name == "AddHostedService" && m.IsGenericMethod);

                            foreach (var taskType in hostedServices)
                            {
                                if (method != null)
                                {
                                    MethodInfo genericMethod = method.MakeGenericMethod(taskType);
                                    genericMethod.Invoke(services, new object[] { services });
                                }
                                else
                                {
                                    Console.WriteLine("AddHostService not found");
                                }
                            }
                        });
                    }
                }
                else
                {
                    var hostedService = typeof(ScheduledTask);
                    var assembly = Assembly.GetExecutingAssembly();

                    var hostedServices = assembly.GetTypes()
                            .Where(t => hostedService.IsAssignableFrom(t) && t.IsClass && !t.IsAbstract && !t.IsInterface).ToList();

                    var method = typeof(ServiceCollectionHostedServiceExtensions)
                            .GetMethods(BindingFlags.Static | BindingFlags.Public)
                            .FirstOrDefault(m => m.Name == "AddHostedService" && m.IsGenericMethod);

                    foreach (var taskType in hostedServices)
                    {
                        if (method != null)
                        {
                            MethodInfo genericMethod = method.MakeGenericMethod(taskType);
                            genericMethod.Invoke(builder.Services, new object[] { builder.Services });
                        }
                        else
                        {
                            Console.WriteLine("AddHostService not found");
                        }
                    }
                }

                if (Configuration.EnableServices && !Configuration.EnableAPI)
                {
                    hostBuilder?.UseWindowsService().Build().Run();
                }
                var app = builder.Build();

                if (Configuration.EnableAPI)
                {
                    app.UseSwagger();
                    app.UseSwaggerUI();
                    app.UseCors();
                    app.UseHttpsRedirection();
                    app.UseAuthorization();
                    app.MapControllers();
                }

                app.Run();
            }
            catch (Exception ex)
            {
                //File.WriteAllText(@"C:\Users\Nikolay Petkov\OneDrive\Desktop\New folder\error.txt", ex.StackTrace.ToString());
            }
        }

        public static class Configuration
        {
            public static bool IsDebuging { get; set; } = false;
            public static string ConnectionString { get; set; }
            public static bool EnableServices { get; set; } = true;
            public static bool EnableAPI { get; set; } = false;

            public static IConfiguration Appsettings { get; set; }


        }
    }
}
