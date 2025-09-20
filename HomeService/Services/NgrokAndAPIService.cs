
using DataLayer;
using Extensions;
using Operations.Ngrok;
using Operations.NgrokAndAPI;

namespace HomeService.Services
{
    public class NgrokAndAPIService : IHostedService
    {

        private readonly NgrokOperations _ngrok;
        private readonly StartAPIOperations _startAPIOperations;

        public NgrokAndAPIService(NgrokOperations ngrok, StartAPIOperations api)
        {
            _ngrok = ngrok;
            _startAPIOperations = api;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            //using (var db = new DataBaseContext())
            //{
            //    int id = await _startAPIOperations.Run();
            //    db.Settings.FirstOrDefault(w => w.Name == "PortId")!.Value = id.ToString();
            //    db.SaveChanges();
            //    await _startAPIOperations.GetListeningPortForProcess(id);

            //    await _ngrok.Run();
            //}
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            //try
            //{
            //    using (var db = new DataBaseContext())
            //    {

            //        var portId = db.Settings.FirstOrDefault(w => w.Name == "PortId")!.Value;
            //        if (portId != null && int.TryParse(portId, out int pid))
            //        {
            //            await _startAPIOperations.Kill(pid);
            //            await _ngrok.Kill();
            //            WriteLog.Log("API STOPPED !");
            //        }
            //    }
            //}
            //catch (Exception ex)
            //{
            //    WriteLog.Log(ex.Message, ex.StackTrace!);
            //}
        }
    }
}
