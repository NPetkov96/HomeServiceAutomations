namespace DataLayer.Models.DTOs
{
    public class MedSestriStatisticsDTO
    {
        public DateTime? FirstDate { get; set; }
        public DateTime? LastDate { get; set; }
        public int TotalDays { get; set; }
        public int ActiveDays { get; set; }
        public decimal AveragePerActiveDay { get; set; }
        public int MaximumPerDay { get; set; }
        public MedSestriPeriodStatisticsDTO Last7Days { get; set; } = new();
        public MedSestriPeriodStatisticsDTO Last30Days { get; set; } = new();
        public List<MedSestriDailyStatisticsDTO> Daily { get; set; } = new();
        public List<MedSestriMonthlyStatisticsDTO> Monthly { get; set; } = new();
        public List<MedSestriWeekdayStatisticsDTO> ByWeekday { get; set; } = new();
    }

    public class MedSestriPeriodStatisticsDTO
    {
        public int RecordCount { get; set; }
        public int PreviousRecordCount { get; set; }
        public decimal? ChangePercent { get; set; }
    }

    public class MedSestriDailyStatisticsDTO
    {
        public DateTime Date { get; set; }
        public int RecordCount { get; set; }
        public decimal SevenDayAverage { get; set; }
    }

    public class MedSestriMonthlyStatisticsDTO
    {
        public DateTime Month { get; set; }
        public int RecordCount { get; set; }
        public int ActiveDays { get; set; }
        public decimal AveragePerActiveDay { get; set; }
        public int MaximumPerDay { get; set; }
    }

    public class MedSestriWeekdayStatisticsDTO
    {
        public int DayNumber { get; set; }
        public string DayName { get; set; } = string.Empty;
        public int RecordCount { get; set; }
    }
}
