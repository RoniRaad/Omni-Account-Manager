using System.Text.Json.Serialization;

namespace AccountManager.Core.Models.RiotGames.League
{
    public sealed class LeagueAccount
    {
        [JsonPropertyName("puuid")]
        public string? Puuid { get; set; }
        [JsonPropertyName("username")]
        public string? Username { get; set; }
    }
}
