using AccountManager.Core.Models.RiotGames.Valorant.Responses;
using System.Text.Json.Serialization;

namespace AccountManager.Core.Models.RiotGames.Valorant
{
    public class TokenResponseWrapper
    {
        [JsonPropertyName("response")]
        public TokenResponse Response { get; set; }
    }
}