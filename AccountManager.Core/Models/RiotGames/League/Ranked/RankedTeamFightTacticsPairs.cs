﻿using System.Text.Json.Serialization;

namespace AccountManager.Core.Models.RiotGames.League
{
    public sealed class RankedTeamFightTacticsPairs
    {
        [JsonPropertyName("currentSeasonId")]
        public int CurrentSeasonId { get; set; }

        [JsonPropertyName("currentSeasonEnd")]
        public long CurrentSeasonEnd { get; set; }

        [JsonPropertyName("nextSeasonStart")]
        public int NextSeasonStart { get; set; }
    }
}
