using System.Text.Json.Serialization;

namespace AccountManager.Core.Models.RiotGames.League
{
    public class Seasons
    {
        [JsonPropertyName("RANKED_TFT")]
        public RankedTeamFightTactics? RankedTFT { get; set; }

        [JsonPropertyName("RANKED_TFT_TURBO")]
        public RankedTeamFightTacticsTurbo? RankedTFTTurbo { get; set; }

        [JsonPropertyName("RANKED_TFT_PAIRS")]
        public RankedTeamFightTacticsPairs? RankedTFTPairs { get; set; }

        [JsonPropertyName("RANKED_FLEX_SR")]
        public RankedTeamFightTacticsFlex? RankedTFTFlex { get; set; }

        [JsonPropertyName("RANKED_SOLO_5x5")]
        public RankedSolo5x5? RankedSolo5x5 { get; set; }
    }
}
