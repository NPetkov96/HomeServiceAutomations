
using DataLayer;
using DataLayer.Models.ImotBg;
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

                    await _imotBgValidation.ValidateData(db);

                    if (!db.ImotBgStatistics.Any(x => x.Date >= DateTime.Today))
                    {
                        var log = new ImotBgStatistic();

                        var apartments = db.ImotBgApartments
                            .AsNoTracking()
                            .ToList();

                        log.Average = Math.Round((double)apartments
                            .Average(x => x.PricePerSqMetre)!, 2);

                        var mladost = apartments
                            .Where(x => x.Neighbour.Contains("Младост"))
                            .ToList();

                        log.Mladost = Math.Round((double)mladost.Average(x => x.PricePerSqMetre)!, 2);

                        log.MladostPanel = Math.Round((double)mladost
                            .Where(x => x.Construction.Contains("Панел"))
                            .Average(x => x.PricePerSqMetre)!, 2);

                        log.MladostTuhla = Math.Round((double)mladost
                            .Where(x => x.Construction.Contains("Тухла"))
                            .Average(x => x.PricePerSqMetre)!, 2);

                        log.MalinovaDolina = Math.Round((double)apartments
                            .Where(x => x.Neighbour.Contains("Малинова долина"))
                            .Average(x => x.PricePerSqMetre)!, 2);

                        if (db.ImotBgStatistics.Any(x => x.Date >= DateTime.Today.AddDays(-1)))
                        {
                            var imotBgStatistics = db.ImotBgStatistics
                                .AsNoTracking()
                                .Where(x => x.Date >= DateTime.Today.AddDays(-1) && x.Date <= DateTime.Today);

                            var compareAverage = imotBgStatistics
                                .Average(x => x.Average);

                            var averageMladsotCompare = imotBgStatistics
                                .Average(x => x.Mladost);

                            var averageMladsotTuhlaCompare = imotBgStatistics
                                .Average(x => x.MladostTuhla);

                            var averageMladsotPanelCompare = imotBgStatistics
                                .Average(x => x.MladostPanel);

                            var averageMalinovaDolinaCompare = imotBgStatistics
                                .Average(x => x.MalinovaDolina);


                            if (compareAverage != log.Average)
                                log.CompareAverage = compareAverage < log.Average ? "UP" : "DOWN";

                            if (averageMladsotCompare != log.Mladost)
                                log.CompareMladost = averageMladsotCompare < log.Mladost ? "UP" : "DOWN";

                            if (averageMladsotTuhlaCompare != log.MladostTuhla)
                                log.CompareMladostTuhla = averageMladsotTuhlaCompare < log.MladostTuhla ? "UP" : "DOWN";

                            if (averageMladsotPanelCompare != log.MladostPanel)
                                log.CompareMladostPanel = averageMladsotPanelCompare < log.MladostPanel ? "UP" : "DOWN";

                            if (averageMalinovaDolinaCompare != log.MalinovaDolina)
                                log.CompareMalinovaDolina = averageMalinovaDolinaCompare < log.MalinovaDolina ? "UP" : "DOWN";

                        }

                        log.Date = DateTime.Now;

                        db.ImotBgStatistics.Add(log);
                        await db.SaveChangesAsync();
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
