using System.Text.Json.Serialization;

namespace AccountManager.Core.Models.RiotGames.League
{
    public sealed class RankedStats
    {
        [JsonPropertyName("tier")]
        public string? Tier { get; set; }
        [JsonPropertyName("division")]
        public string? Division { get; set; }
    }
}
