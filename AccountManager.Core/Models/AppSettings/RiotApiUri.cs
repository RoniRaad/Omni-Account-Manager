using System.Text.Json.Serialization;

namespace AccountManager.Core.Models.AppSettings
{
    public class RiotApiUri
    {
        [JsonPropertyName("Auth")]
        public string? Auth { get; set; }

        [JsonPropertyName("Valorant")]
        public string? Valorant { get; set; }

        [JsonPropertyName("Entitlement")]
        public string? Entitlement { get; set; }

        [JsonPropertyName("LeagueNA")]
        public string? LeagueNA { get; set; }
    }
}
