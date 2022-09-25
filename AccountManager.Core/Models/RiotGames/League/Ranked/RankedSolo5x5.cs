using System.Text.Json.Serialization;

namespace AccountManager.Core.Models.RiotGames.League
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Minor Code Smell", "S101:Types should be named in PascalCase", Justification = "Accurately represents object.")]
    public sealed class RankedSolo5x5
    {
        [JsonPropertyName("currentSeasonId")]
        public int CurrentSeasonId { get; set; }

        [JsonPropertyName("currentSeasonEnd")]
        public long CurrentSeasonEnd { get; set; }

        [JsonPropertyName("nextSeasonStart")]
        public int NextSeasonStart { get; set; }
    }
}
