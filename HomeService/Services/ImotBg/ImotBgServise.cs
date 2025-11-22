
using DataLayer;
using Extensions;
using Operations.ImotBg;
using static HomeService.Program;

namespace HomeService.Services.ImotBg
{
    public class ImotBgServise : ScheduledTask
    {
        private readonly ImotBgOperations _imotBgOperations;

        public ImotBgServise(ImotBgOperations imotBgOperations) : base(Configuration.Appsettings.GetSection("ImotBgServise").GetValue<string>("CronPattern"),
                 Configuration.Appsettings.GetSection("ImotBgServise").GetValue<bool>("ServiceActive"))
        {
            this._imotBgOperations = imotBgOperations;
        }

        protected async override Task ExecuteTask()
        {
            try
            {
                using (var db = new DataBaseContext())
                {
                    await _imotBgOperations.Run(db);
                }
            }
            catch (Exception ex)
            {
                WriteLog.Log(ex.Message, ex.StackTrace!);
            }
        }
    }
}
