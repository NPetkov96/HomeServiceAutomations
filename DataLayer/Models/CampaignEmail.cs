using System.ComponentModel.DataAnnotations;

namespace DataLayer.Models
{

    public enum Status
    {
        ExtractingAssets = 1,
        CouldNotExtractAttachment = 2,
        GeneratingFile = 3,
        SendingFile = 4,
        Completed = 5


    }
    public class CampaignEmail
    {
        [Key]
        public int Id { get; set; }
        public string? EmailId { get; set; }
        public string Client { get; set; }
        public string CampaignId { get; set; }
        public byte[]? Body { get; set; }
        public DateTime? ReceivedDate { get; set; }
        public bool IsCompleted { get; set; } = false;
        public List<CampaignAsset> Assets { get; set; }
        public Status EmailStatus { get; set; }
        public string? AttachmentPath { get; set; }
    }
}
