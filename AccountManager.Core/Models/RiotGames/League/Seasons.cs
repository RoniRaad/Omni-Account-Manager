using System.Text.Json.Serialization;

namespace AccountManager.Core.Models.RiotGames.League
{
    public class Seasons
    {
        [JsonPropertyName("RANKED_TFT")]
        public RankedTFT RankedTFT { get; set; }

        [JsonPropertyName("RANKED_TFT_TURBO")]
        public RankedTFTTurbo RankedTFTTurbo { get; set; }

        [JsonPropertyName("RANKED_TFT_PAIRS")]
        public RankedTFTPairs RankedTFTPairs { get; set; }

        [JsonPropertyName("RANKED_FLEX_SR")]
        public RankedTFTFlex RankedTFTFlex { get; set; }

        [JsonPropertyName("RANKED_SOLO_5x5")]
        public RankedSolo5x5 RankedSolo5x5 { get; set; }
    }
}
