using System.Text.Json.Serialization;

namespace AccountManager.Core.Models.RiotGames.Valorant
{
    public sealed class ExpectedClientVersionData
    {
        [JsonPropertyName("riotClientVersion")]
        public string? RiotClientVersion { get; set; }
        [JsonPropertyName("riotClientBuild")]
        public string? RiotClientBuild { get; set; }
    }
}
