using System.ComponentModel.DataAnnotations;

namespace DataLayer.Models
{
    public class CampaignPlatform
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }
        public string? CPC { get; set; }
        public string? CostPerPostEngagment { get; set; }
        public ICollection<CampaignClientPlatform> ClientPlatforms { get; set; } = new List<CampaignClientPlatform>();
    }
}
