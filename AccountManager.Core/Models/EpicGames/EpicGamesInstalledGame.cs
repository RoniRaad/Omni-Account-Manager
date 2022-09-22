using System.Text.Json.Serialization;

namespace AccountManager.Core.Models.EpicGames
{
    public sealed class EpicGamesInstalledGame
    {
        [JsonPropertyName("DisplayName")]
        public string? Name { get; set; }
        [JsonPropertyName("MainGameAppName")]
        public string? AppId { get; set; }
        public string? LaunchExecutable { get; set; }
        public string? InstallLocation { get; set; }
    }
}
