using System.ComponentModel.DataAnnotations;

namespace DataLayer.Models
{
    public class CampaignSettings
    {
        [Key]
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? Value { get; set; }
    }
}
