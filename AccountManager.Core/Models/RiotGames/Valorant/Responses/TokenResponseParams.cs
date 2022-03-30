using System.Text.Json.Serialization;

namespace AccountManager.Core.Models.RiotGames.Valorant.Responses
{
    public class TokenResponseParams
    {
        [JsonPropertyName("parameters")]
        public TokenParameters Parameters { get; set; }
    }
}