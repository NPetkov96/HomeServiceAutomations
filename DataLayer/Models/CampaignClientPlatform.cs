using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataLayer.Models
{
    public class CampaignClientPlatform
    {
        [Key]
        public int Id { get; set; }


        [ForeignKey("Client")]
        public int ClientId { get; set; }
        public CampaignClient Client { get; set; }

        [ForeignKey("Platform")]
        public int PlatformId { get; set; }
        public CampaignPlatform Platform { get; set; }

        public double PercentFee { get; set; }
    }
}
