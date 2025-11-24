
using DataLayer;
using Extensions;
using Microsoft.EntityFrameworkCore;
using Operations.ImotBg;
using static HomeService.Program;

namespace HomeService.Services.ImotBg
{
    public class ImotBgServise : ScheduledTask
    {
        private readonly ImotBgScraping _imotBgScraping;
        private readonly ImotBgValidation _imotBgValidation;

        public ImotBgServise(ImotBgScraping imotBgOperations, ImotBgValidation imotBgValidation) : base(Configuration.Appsettings.GetSection("ImotBgServise").GetValue<string>("CronPattern"),
                 Configuration.Appsettings.GetSection("ImotBgServise").GetValue<bool>("ServiceActive"))
        {
            this._imotBgScraping = imotBgOperations;
            this._imotBgValidation = imotBgValidation;
        }

        protected async override Task ExecuteTask()
        {
            try
            {
                using (var db = new DataBaseContext())
                {
                    await _imotBgScraping.StartScraping(db);

                    var apartmnets = db.ImotBgApartments.AsNoTracking().ToList();

                    //double averagePrice = apartmnets.Average(x => (double)x.Price);
                    //double averagePricePerSquare = apartmnets.Average(x => (double)x.PricePerSqMetre);

                    //Console.WriteLine(Math.Round(averagePrice,2));
                    //Console.WriteLine(Math.Round(averagePricePerSquare, 2));
                }
            }
            catch (Exception ex)
            {
                WriteLog.Log(ex.Message, ex.StackTrace!);
            }
        }
    }
}
