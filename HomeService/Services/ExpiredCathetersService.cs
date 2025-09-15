using DataLayer;
using Extensions;
using Operations.Catheters;
using static HomeService.Program;

namespace HomeService.Services
{
    public class ExpiredCathetersService : ScheduledTask
    {

        private readonly SendCatheterNotificationOperation _sendCatheterNotificationOperation;

        public ExpiredCathetersService(SendCatheterNotificationOperation sendCatheterNotificationOperation) : base(Configuration.Appsettings.GetSection("ExpiredCathetersService").GetValue<string>("CronPattern"),
                  Configuration.Appsettings.GetSection("ExpiredCathetersService").GetValue<bool>("ServiceActive"))
        {
            _sendCatheterNotificationOperation = sendCatheterNotificationOperation;
        }

        protected async override Task ExecuteTask()
        {
            try
            {
                var mounthAgo = DateTime.Now.AddMonths(-1);

                using (var db = new DataBaseContext())
                {
                    var expiredCatheters = db.MedSestriCatheters
                        .Where(c => c.Date <= mounthAgo)
                        .ToList();

                    if (expiredCatheters.Any())
                    {
                        foreach (var catheter in expiredCatheters)
                        {
                            catheter.IsOverdue = true;
                            db.SaveChanges();

                            await _sendCatheterNotificationOperation.Send(catheter);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                WriteLog.Log(ex.Message, ex.StackTrace!);
            }
        }
    }
}
