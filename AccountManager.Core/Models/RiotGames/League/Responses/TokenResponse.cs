using System.Text.Json.Serialization;

namespace AccountManager.Core.Models.RiotGames.League.Responses
{
    public class TokenResponse
    {
        [JsonPropertyName("token")]
        public string Token { get; set; }
    }
}
