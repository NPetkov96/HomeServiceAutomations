using System.Text.Json.Serialization;

namespace DataLayer.Models
{
    public class KPICalculationsDTO
    {
        [JsonPropertyName("data")]
        public List<CpcCalculationDtoRaw> Data { get; set; }

        [JsonPropertyName("paging")]
        public Paging Paging { get; set; }
    }
    public class CpcCalculationDtoRaw
    {
        [JsonPropertyName("campaign_id")]
        public string CampaignId { get; set; }

        [JsonPropertyName("campaign_name")]
        public string CampaignName { get; set; }

        [JsonPropertyName("cpc")]
        public string Cpc { get; set; }

        [JsonPropertyName("objective")]
        public string Objective { get; set; }

        [JsonPropertyName("spend")]
        public string Spend { get; set; }

        [JsonPropertyName("actions")]
        public List<ActionItem> Actions { get; set; }

        [JsonPropertyName("publisher_platform")]
        public string PublisherPlatform { get; set; }

        [JsonPropertyName("platform_position")]
        public string PublisherPosition { get; set; }
    }

    public class ActionItem
    {
        [JsonPropertyName("action_type")]
        public string ActionType { get; set; }

        [JsonPropertyName("value")]
        public string Value { get; set; }
    }

    public class Paging
    {
        [JsonPropertyName("cursors")]
        public PagingCursors Cursors { get; set; }
    }

    public class PagingCursors
    {
        [JsonPropertyName("before")]
        public string Before { get; set; }

        [JsonPropertyName("after")]
        public string After { get; set; }
    }
}
