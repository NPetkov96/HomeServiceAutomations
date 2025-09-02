namespace DataLayer.Models
{
    public class CampaignAsset
    {
        public int Id { get; set; }
        public string? Client { get; set; }
        public string? Product { get; set; }
        public string? CampaignId { get; set; }
        public string? Platform { get; set; }
        public string Target { get; set; }
        public string Optimization { get; set; }
        public string Formats { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string Period { get; set; }
        public decimal Budget { get; set; }
    }
}
