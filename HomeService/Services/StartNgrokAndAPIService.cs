using DataLayer;
using Extensions;
using Operations.Ngrok;
using Operations.NgrokAndAPI;
using static HomeService.Program;

namespace HomeService.Services
{
    public class StartNgrokAndAPIService : ScheduledTask
    {
        private readonly NgrokOperations _ngrok;
        private readonly StartAPIOperations _startAPIOperations;

        public StartNgrokAndAPIService(NgrokOperations ngrok, StartAPIOperations startAPIOperations) : base(Configuration.Appsettings.GetSection("StartNgrokAndAPIService").GetValue<string>("CronPattern"),
                  Configuration.Appsettings.GetSection("StartNgrokAndAPIService").GetValue<bool>("ServiceActive"))
        {
            _ngrok = ngrok;
            _startAPIOperations = startAPIOperations;
        }

        protected async override Task ExecuteTask()
        {
            try
            {
                using (var db = new DataBaseContext())
                {
                    var url = db.Settings.FirstOrDefault(w => w.Name == "NgrokDNS")!.Value;
                    var client = new HttpClient();
                    var resposne = await client.GetAsync($"{url}/api/Bodimed/allbloodtests");
                    if (resposne.IsSuccessStatusCode)
                    {
                        await _ngrok.Kill();
                        await _startAPIOperations.Kill(int.Parse(db.Settings.FirstOrDefault(w => w.Name == "PortId")!.Value!));
                    }

                    int id = await _startAPIOperations.Run();
                    db.Settings.FirstOrDefault(w => w.Name == "PortId")!.Value = id.ToString();
                    db.SaveChanges();
                    await _startAPIOperations.GetListeningPortForProcess(id);

                    await _ngrok.Run();

                    WriteLog.Log("Api Host is reset successfully");
                }
            }
            catch (Exception ex)
            {
                WriteLog.Log(ex.Message, ex.StackTrace!);
            }
        }
    }
}
