using DataLayer;
using DataLayer.Models.ImotBg;
using Extensions;
using Microsoft.EntityFrameworkCore;

namespace Operations.ImotBg
{
    public class DailyImotBgOperation
    {
        private readonly ImotBgScraping _scraping;
        private readonly ImotBgValidation _validation;

        public DailyImotBgOperation(ImotBgScraping scraping, ImotBgValidation validation)
        {
            _scraping = scraping;
            _validation = validation;
        }

        public async Task Run()
        {
            using var db = new DataBaseContext();

            await _scraping.StartScraping(db);
            await _validation.ValidateData(db);

            if (await db.ImotBgStatistics.AnyAsync(x => x.Date >= DateTime.Today))
            {
                WriteLog.Log("ImotBg daily statistics already exist. Skipping duplicate run.");
                return;
            }

            var apartments = await db.ImotBgApartments.AsNoTracking().ToListAsync();
            if (apartments.Count == 0)
            {
                WriteLog.Log("No ImotBg apartments were found for daily statistics.");
                return;
            }

            var mladost = apartments
                .Where(x => x.Neighbour?.Contains("Младост") == true)
                .ToList();

            var statistic = new ImotBgStatistic
            {
                Average = Average(apartments.Select(x => x.PricePerSqMetre)),
                Mladost = Average(mladost.Select(x => x.PricePerSqMetre)),
                MladostPanel = Average(mladost.Where(x => x.Construction?.Contains("Панел") == true).Select(x => x.PricePerSqMetre)),
                MladostTuhla = Average(mladost.Where(x => x.Construction?.Contains("Тухла") == true).Select(x => x.PricePerSqMetre)),
                MalinovaDolina = Average(apartments.Where(x => x.Neighbour?.Contains("Малинова долина") == true).Select(x => x.PricePerSqMetre)),
                Date = DateTime.Now
            };

            var previous = await db.ImotBgStatistics
                .AsNoTracking()
                .Where(x => x.Date >= DateTime.Today.AddDays(-1) && x.Date < DateTime.Today)
                .OrderByDescending(x => x.Date)
                .FirstOrDefaultAsync();

            if (previous is not null)
            {
                statistic.CompareAverage = Compare(previous.Average, statistic.Average);
                statistic.CompareMladost = Compare(previous.Mladost, statistic.Mladost);
                statistic.CompareMladostTuhla = Compare(previous.MladostTuhla, statistic.MladostTuhla);
                statistic.CompareMladostPanel = Compare(previous.MladostPanel, statistic.MladostPanel);
                statistic.CompareMalinovaDolina = Compare(previous.MalinovaDolina, statistic.MalinovaDolina);
            }

            db.ImotBgStatistics.Add(statistic);
            await db.SaveChangesAsync();
            WriteLog.Log("ImotBg daily operation completed successfully.");
        }

        private static double Average(IEnumerable<double?> values)
        {
            var availableValues = values
                .Where(value => value.HasValue)
                .Select(value => value!.Value)
                .ToList();

            return availableValues.Count == 0 ? 0 : Math.Round(availableValues.Average(), 2);
        }

        private static string? Compare(double previous, double current)
        {
            return previous == current ? null : previous < current ? "UP" : "DOWN";
        }
    }
}
