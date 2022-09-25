using System.Text.Json.Serialization;

namespace AccountManager.Core.Models.RiotGames
{
    public sealed class RiotAuthTokenParameters
    {
        [JsonPropertyName("uri")]
        public string? Uri { get; set; }
    }
}
