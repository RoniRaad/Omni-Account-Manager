using System.Text.Json.Serialization;

namespace AccountManager.Core.Models.RiotGames.League.Responses
{
    public class LeagueSessionResponse
    {
        [JsonPropertyName("token")]
        public string Token { get; set; }
    }
}
