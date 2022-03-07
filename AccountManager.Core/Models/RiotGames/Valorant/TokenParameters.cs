using System.Text.Json.Serialization;

namespace AccountManager.Core.Models.RiotGames.Valorant
{
    public class TokenParameters
    {
        [JsonPropertyName("uri")]
        public string Uri { get; set; }
    }
}

