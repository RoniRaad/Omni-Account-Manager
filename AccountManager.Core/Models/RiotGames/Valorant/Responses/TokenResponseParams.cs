using System.Text.Json.Serialization;

namespace AccountManager.Core.Models.RiotGames.Valorant.Responses
{
    public sealed class TokenResponseParams
    {
        [JsonPropertyName("parameters")]
        public TokenParameters? Parameters { get; set; }
    }
}