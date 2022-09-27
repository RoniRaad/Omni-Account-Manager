using System.Text.Json.Serialization;

namespace AccountManager.Core.Models.RiotGames.Valorant
{
    public sealed class TokenParameters
    {
        [JsonPropertyName("uri")]
        public string? Uri { get; set; }
    }
}

