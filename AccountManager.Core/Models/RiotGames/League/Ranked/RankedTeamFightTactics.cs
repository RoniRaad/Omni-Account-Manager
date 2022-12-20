using System.Text.Json.Serialization;

namespace AccountManager.Core.Models.RiotGames.League
{
    public sealed class RankedTeamFightTactics
    {
        [JsonPropertyName("currentSeasonId")]
        public long CurrentSeasonId { get; set; }

        [JsonPropertyName("currentSeasonEnd")]
        public long CurrentSeasonEnd { get; set; }

        [JsonPropertyName("nextSeasonStart")]
        public long NextSeasonStart { get; set; }
    }
}
