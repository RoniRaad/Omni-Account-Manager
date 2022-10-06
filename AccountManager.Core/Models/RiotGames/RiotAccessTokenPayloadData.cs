using System.Text.Json.Serialization;

namespace AccountManager.Core.Models.RiotGames
{
    public sealed class RiotAccessTokenPayloadData
    {
        [JsonPropertyName("r")]
        public string Region { get; set; } = "NA1";
    }
}
