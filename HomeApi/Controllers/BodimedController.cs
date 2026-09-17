using DataLayer;
using DataLayer.Models;
using DataLayer.Models.DTOs;
using Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HomeApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BodimedController : ControllerBase
    {
        private readonly DataBaseContext _context;

        public BodimedController(DataBaseContext context)
        {
            this._context = context;
        }

        [HttpGet("allBloodTests")] // SEND ALL BLOOD TESTs 
        public async Task<List<MedSestriBloodTest>> GetAllBloodTestsAsync()
        {
            return await _context.MedSestriBloodTests
                .OrderByDescending(x => x.HasPriority)
                .ThenBy(x => x.Id)
                .ToListAsync();
        }

        [HttpGet("allPatientsHistory")] // SEND ALL PATIENTs 
        public async Task<List<MedSestriPatientsDTO>> GetAllPatientsHistoryAsync()
        {
            var patients = await _context.MedSestriPatients
                .OrderByDescending(p => p.Date)
                .Include(p => p.PatientBloodTests)
                .ThenInclude(pt => pt.BloodTest)
                .ToListAsync();

            var patientsDto = patients.Select(patient => new MedSestriPatientsDTO
            {
                FullName = patient.FullName,
                EGN = patient.EGN,
                PhoneNumber = patient.PhoneNumber,
                Date = patient.Date,
                Note = patient.Note,
                BloodTests = patient.PatientBloodTests.Select(pb => new MedSestriBloodTest
                {
                    Name = pb.BloodTest.Name,
                    BngPrice = pb.BloodTest.BngPrice,
                    EuroPrice = pb.BloodTest.EuroPrice
                })
                .ToList()
            }).ToList();

            return patientsDto;
        }

        [HttpGet("statistics")]
        public async Task<ActionResult<MedSestriStatisticsDTO>> GetStatisticsAsync(
            CancellationToken cancellationToken)
        {
            var activeDays = await _context.MedSestriPatients
                .AsNoTracking()
                .GroupBy(patient => patient.Date.Date)
                .Select(group => new
                {
                    Date = group.Key,
                    RecordCount = group.Count()
                })
                .OrderBy(day => day.Date)
                .ToListAsync(cancellationToken);

            if (activeDays.Count == 0)
                return Ok(new MedSestriStatisticsDTO());

            var today = GetSofiaToday();
            var firstDate = activeDays[0].Date.Date;
            var lastDate = activeDays[^1].Date.Date;
            var recordsByDate = activeDays.ToDictionary(day => day.Date.Date, day => day.RecordCount);
            var dailySeries = BuildDailySeries(firstDate, today, recordsByDate);

            var result = new MedSestriStatisticsDTO
            {
                FirstDate = firstDate,
                LastDate = lastDate,
                TotalDays = Math.Max(0, (today - firstDate).Days + 1),
                ActiveDays = activeDays.Count,
                AveragePerActiveDay = Math.Round(
                    activeDays.Sum(day => day.RecordCount) / (decimal)activeDays.Count,
                    2),
                MaximumPerDay = activeDays.Max(day => day.RecordCount),
                Last7Days = BuildPeriodStatistics(recordsByDate, lastDate, 7),
                Last30Days = BuildPeriodStatistics(recordsByDate, lastDate, 30),
                Daily = dailySeries,
                Monthly = activeDays
                    .GroupBy(day => new { day.Date.Year, day.Date.Month })
                    .OrderBy(group => group.Key.Year)
                    .ThenBy(group => group.Key.Month)
                    .Select(group => new MedSestriMonthlyStatisticsDTO
                    {
                        Month = new DateTime(group.Key.Year, group.Key.Month, 1),
                        RecordCount = group.Sum(day => day.RecordCount),
                        ActiveDays = group.Count(),
                        AveragePerActiveDay = Math.Round(group.Average(day => (decimal)day.RecordCount), 2),
                        MaximumPerDay = group.Max(day => day.RecordCount)
                    })
                    .ToList(),
                ByWeekday = activeDays
                    .GroupBy(day => GetMondayBasedDayNumber(day.Date.DayOfWeek))
                    .OrderBy(group => group.Key)
                    .Select(group => new MedSestriWeekdayStatisticsDTO
                    {
                        DayNumber = group.Key,
                        DayName = GetBulgarianDayName(group.Key),
                        RecordCount = group.Sum(day => day.RecordCount)
                    })
                    .ToList()
            };

            return Ok(result);
        }

        private static List<MedSestriDailyStatisticsDTO> BuildDailySeries(
            DateTime firstDate,
            DateTime today,
            IReadOnlyDictionary<DateTime, int> recordsByDate)
        {
            var result = new List<MedSestriDailyStatisticsDTO>();
            var rollingWindow = new Queue<int>();
            var rollingSum = 0;

            for (var date = firstDate; date <= today; date = date.AddDays(1))
            {
                var recordCount = recordsByDate.GetValueOrDefault(date);
                rollingWindow.Enqueue(recordCount);
                rollingSum += recordCount;

                if (rollingWindow.Count > 7)
                    rollingSum -= rollingWindow.Dequeue();

                result.Add(new MedSestriDailyStatisticsDTO
                {
                    Date = date,
                    RecordCount = recordCount,
                    SevenDayAverage = Math.Round(rollingSum / (decimal)rollingWindow.Count, 2)
                });
            }

            return result;
        }

        private static MedSestriPeriodStatisticsDTO BuildPeriodStatistics(
            IReadOnlyDictionary<DateTime, int> recordsByDate,
            DateTime lastDate,
            int numberOfDays)
        {
            var currentStartExclusive = lastDate.AddDays(-numberOfDays);
            var previousStartExclusive = lastDate.AddDays(-(numberOfDays * 2));
            var previousEndInclusive = currentStartExclusive;

            var currentCount = recordsByDate
                .Where(day => day.Key > currentStartExclusive && day.Key <= lastDate)
                .Sum(day => day.Value);

            var previousCount = recordsByDate
                .Where(day => day.Key > previousStartExclusive && day.Key <= previousEndInclusive)
                .Sum(day => day.Value);

            return new MedSestriPeriodStatisticsDTO
            {
                RecordCount = currentCount,
                PreviousRecordCount = previousCount,
                ChangePercent = previousCount == 0
                    ? null
                    : Math.Round((currentCount - previousCount) * 100m / previousCount, 1)
            };
        }

        private static DateTime GetSofiaToday()
        {
            foreach (var timeZoneId in new[] { "Europe/Sofia", "FLE Standard Time" })
            {
                try
                {
                    var sofiaTimeZone = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
                    return TimeZoneInfo.ConvertTime(DateTimeOffset.UtcNow, sofiaTimeZone).Date;
                }
                catch (TimeZoneNotFoundException)
                {
                }
                catch (InvalidTimeZoneException)
                {
                }
            }

            return DateTime.UtcNow.Date;
        }

        private static int GetMondayBasedDayNumber(DayOfWeek dayOfWeek) =>
            ((int)dayOfWeek + 6) % 7 + 1;

        private static string GetBulgarianDayName(int dayNumber) => dayNumber switch
        {
            1 => "Понеделник",
            2 => "Вторник",
            3 => "Сряда",
            4 => "Четвъртък",
            5 => "Петък",
            6 => "Събота",
            7 => "Неделя",
            _ => string.Empty
        };

        [HttpPost("createPatient")] // CREATING NEW PATIENT
        public async Task CreatePatient([FromBody] MedSestriPatient model)
        {
            var oneHourAgo = DateTime.Now.AddHours(-1);

            var existingModel = await _context.MedSestriPatients
                .FirstOrDefaultAsync(p => p.Date >= oneHourAgo && (p.EGN == model.EGN || p.FullName == model.FullName));

            var newBloodTests = model.BloodTests
                .Select(bt => bt.Name)
                .Where(name => !string.IsNullOrWhiteSpace(name))
                .Distinct(StringComparer.Ordinal)
                .ToList();

            var selectedBloodTestIds = await _context.MedSestriBloodTests
                .AsNoTracking()
                .Where(bt => newBloodTests.Contains(bt.Name))
                .Select(bt => bt.Id)
                .ToListAsync();

            if (existingModel != null)
            {
                existingModel.FullName = model.FullName;
                existingModel.PhoneNumber = model.PhoneNumber;
                existingModel.EGN = model.EGN;
                existingModel.Date = model.Date;
                existingModel.Note = model.Note;

                var existingRelations = await _context.MedSestriPatientsBloodTests
                    .Where(x => x.PatientId == existingModel.Id)
                    .ToListAsync();

                var selectedBloodTestIdSet = selectedBloodTestIds.ToHashSet();
                var existingBloodTestIdSet = existingRelations
                    .Select(relation => relation.BloodTestId)
                    .ToHashSet();

                _context.MedSestriPatientsBloodTests.RemoveRange(
                    existingRelations.Where(relation =>
                        !selectedBloodTestIdSet.Contains(relation.BloodTestId)));
                _context.MedSestriPatientsBloodTests.AddRange(
                    selectedBloodTestIds
                        .Where(bloodTestId =>
                            !existingBloodTestIdSet.Contains(bloodTestId))
                        .Select(bloodTestId =>
                        new MedSestriPatientBloodTest
                        {
                            PatientId = existingModel.Id,
                            BloodTestId = bloodTestId
                        }));
                WriteLog.Log($"Patient {model.FullName} updated with new blood tests.");
            }
            else
            {
                var newPatient = new MedSestriPatient
                {
                    FullName = model.FullName,
                    PhoneNumber = model.PhoneNumber,
                    EGN = model.EGN,
                    Date = model.Date,
                    Note = model.Note,
                    PatientBloodTests = selectedBloodTestIds
                        .Select(bloodTestId => new MedSestriPatientBloodTest
                        {
                            BloodTestId = bloodTestId
                        })
                        .ToList()
                };

                await _context.MedSestriPatients.AddAsync(newPatient);
                WriteLog.Log($"New patient {model.FullName} created with blood tests.");
            }

            await _context.SaveChangesAsync();
        }

        [HttpPost("deletePatient")] // DELETE PATIENT
        public async Task DeletePatient([FromBody] DateTime date)
        {
            var patient = await _context.MedSestriPatients
                .FirstOrDefaultAsync(p => p.Date == date);

            var existingRelations = await _context.MedSestriPatientsBloodTests
                .Where(x => x.PatientId == patient!.Id)
                .ToListAsync();

            _context.MedSestriPatientsBloodTests.RemoveRange(existingRelations);
            await _context.SaveChangesAsync();

            _context.MedSestriPatients.Remove(patient!);
            await _context.SaveChangesAsync();
            WriteLog.Log($"Patient {patient!.FullName} deleted.");
        }

        [HttpPost("createCatheterAppointment")]
        public async Task CreateCatheterAppointment([FromBody] MedSestriCatheter model)
        {
            await _context.MedSestriCatheters.AddAsync(model);
            await _context.SaveChangesAsync();
            WriteLog.Log($"New catheter appointment for {model.ClientName} on {model.Date} created.");
        }

        [HttpGet("getAllCatheterAppointments")]
        public async Task<List<MedSestriCatheter>> GetAllCatheterAppointments()
        {
            var result = await _context.MedSestriCatheters
                .Where(p => p.IsChecked == false)
                .OrderBy(d => d.Date)
                .ToListAsync();

            return result;
        }

        [HttpPut("checkCatheterAppointment")]
        public async Task CheckCatheterAppointment([FromBody] MedSestriCatheter model)
        {
            var entity = _context.MedSestriCatheters
                .FirstOrDefault(c => c.Id == model.Id);

            entity.IsChecked = true;
            await _context.SaveChangesAsync();
            WriteLog.Log($"Catheter appointment for {model.ClientName} on {model.Date} checked.");
        }

        [HttpPut("updateCatheterAppointment")]
        public async Task UpdateCatheterAppointment([FromBody] MedSestriCatheter model)
        {
            var entity = _context.MedSestriCatheters
                .FirstOrDefault(c => c.Id == model.Id);

            entity.ClientName = model.ClientName;
            entity.PhoneNumber = model.PhoneNumber;
            entity.Date = model.Date;
            entity.Address = model.Address;
            await _context.SaveChangesAsync();
        }
    }
}
