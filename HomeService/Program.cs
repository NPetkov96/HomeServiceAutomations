using DataLayer;
using Extensions;
using HomeService.Services;
using Operations.BloodTetsUpdate;
using Operations.Catheters;
using Operations.ImotBg;
using Operations.Ngrok;
using Operations.NgrokAndAPI;
using Operations.UpdateKPIResults;
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
                Configuration.ConnectionString = builder.Configuration.GetConnectionString("ConnectionString")!;

                builder.Services.AddScoped<DataBaseContext>();

                IHostBuilder hostBuilder = null;
                hostBuilder = Host.CreateDefaultBuilder(args).ConfigureServices((hostContext, services) =>
                {
                    //Campaigns
                    services.AddScoped<Operations.NoniCampaignsAttachemnts.CampaignsOperations>();
                    services.AddScoped<Operations.NoniCampaignsAttachemnts.Emails>();
                    services.AddScoped<Operations.NoniCampaignsAttachemnts.CreateCampaign>();
                    services.AddScoped<Operations.NoniCampaignsAttachemnts.ExtractPropertiesByEmail>();

                    //KPI
                    services.AddScoped<UpdateKPI>();

                    //Blood tests
                    services.AddScoped<UpdateBloodTestsOperation>();

                    //Ngrok
                    services.AddScoped<NgrokOperations>();
                    services.AddScoped<StartAPIOperations>();
                    services.AddHostedService<NgrokAndAPIService>();

                    //Catheters
                    services.AddScoped<SendCatheterNotificationOperation>();

                    //ImotBg
                    services.AddScoped<ImotBgOperations>();

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
                    }
                });

                hostBuilder?.UseWindowsService().Build().Run();

                builder.WebHost.ConfigureKestrel(options =>
                {
                    options.ListenLocalhost(5001);
                });

                var app = builder.Build();

                app.Run();
            }
            catch (Exception ex)
            {
                WriteLog.Log(ex.Message, ex.StackTrace!);
            }
        }

        public static class Configuration
        {
            public static bool IsDebuging { get; set; } = false;
            public static string ConnectionString { get; set; }
            public static bool EnableServices { get; set; } = true;
            public static bool EnableAPI { get; set; } = true;
            public static IConfiguration Appsettings { get; set; }
        }
    }
}
