namespace DataLayer.Models.ImotBg
{
    public class ImotBgStatistic
    {
        public int Id { get; set; }
        public string? CompareAverage { get; set; }
        public double Average { get; set; }
        public string? CompareMladost { get; set; }
        public double Mladost { get; set; }
        public string? CompareMladostTuhla { get; set; }
        public double MladostTuhla { get; set; }
        public string? CompareMladostPanel { get; set; }
        public double MladostPanel { get; set; }
        public string? CompareMalinovaDolina { get; set; }
        public double MalinovaDolina { get; set; }
        public DateTime? Date { get; set; }
    }
}
