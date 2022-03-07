using System.Text.Json.Serialization;

namespace AccountManager.Core.Models.RiotGames.League.Requests
{
    public class PostSessionsRequest
    {
        [JsonPropertyName("claims")]
        public Claims Claims { get; set; }
        [JsonPropertyName("product")]
        public string Product { get; set; }
        [JsonPropertyName("puuid")]
        public string PuuId { get; set; }
        [JsonPropertyName("region")]
        public string Region { get; set; }
    }
}
