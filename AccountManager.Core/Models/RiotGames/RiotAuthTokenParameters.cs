using System.Text.Json.Serialization;

namespace AccountManager.Core.Models.RiotGames
{
    public class RiotAuthTokenParameters
    {
        [JsonPropertyName("uri")]
        public string? Uri { get; set; }
    }
}
