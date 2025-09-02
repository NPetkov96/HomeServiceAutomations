using System.Net.NetworkInformation;
using System.Text.Json.Serialization;

namespace DataLayer.Models
{
    public class KPIClientDTOResponse
    {
        [JsonPropertyName("data")]
        public List<AccountData> Data { get; set; }

        [JsonPropertyName("paging")]
        public Paging Paging { get; set; }
    }
    public class AccountData
    {
        [JsonPropertyName("account_id")]
        public string AccountId { get; set; }

        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }
    }

    public class Cursors
    {
        [JsonPropertyName("before")]
        public string Before { get; set; }

        [JsonPropertyName("after")]
        public string After { get; set; }
    }
}
