using System.Text.Json.Serialization;

namespace AccountManager.Core.Models.RiotGames.Valorant
{
    public class ExpectedClientVersionData
    {
        [JsonPropertyName("riotClientVersion")]
        public string? RiotClientVersion { get; set; }

    }
}
