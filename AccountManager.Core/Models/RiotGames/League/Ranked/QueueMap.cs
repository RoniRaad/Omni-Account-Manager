using System.Text.Json.Serialization;

namespace AccountManager.Core.Models.RiotGames.League
{
    public sealed class QueueMap
    {
        [JsonPropertyName("RANKED_SOLO_5x5")]
        public RankedStats? RankedSoloDuoStats { get; set; }
        [JsonPropertyName("RANKED_TFT")]
        public RankedStats? TFTStats { get; set; }
    }
}
