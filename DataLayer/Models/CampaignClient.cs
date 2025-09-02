using System.ComponentModel.DataAnnotations;

namespace DataLayer.Models
{
    public class CampaignClient
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }
        public ICollection<CampaignClientPlatform> ClientPlatforms { get; set; } = new List<CampaignClientPlatform>();
    }
}
