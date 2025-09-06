
using Extensions;
using Operations.Ngrok;
using Operations.NgrokAndAPI;

namespace HomeService.Services
{
    public class StоpNgrokAndAPIService : IHostedService
    {

        private readonly NgrokOperations _ngrok;
        private readonly StartAPIOperations _startAPIOperations;

        public StоpNgrokAndAPIService(NgrokOperations ngrok, StartAPIOperations api)
        {
            _ngrok = ngrok;
            _startAPIOperations = api;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            try
            {
                using var db = new DataLayer.DataBaseContext();

                var portId = db.Settings.FirstOrDefault(w => w.Name == "PortId")!.Value;
                if (portId != null && int.TryParse(portId, out int pid))
                {
                    await _startAPIOperations.Kill(pid);
                    await _ngrok.Kill();
                    WriteLog.Log("API STOPPED !");
                }

            }
            catch (Exception ex)
            {
                WriteLog.Log(ex.Message, ex.StackTrace!);
            }
        }
    }
}
