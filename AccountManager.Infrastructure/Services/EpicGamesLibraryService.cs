using AccountManager.Core.Interfaces;
using AccountManager.Core.Models.UserSettings;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace AccountManager.Infrastructure.Services
{
    public class EpicGamesLibraryService : IEpicGamesLibraryService
    {
        private readonly IUserSettingsService<GeneralSettings> _userSettings;
        private string EpicGamesManifestPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
            "Epic", "EpicGamesLauncher", "Data", "Manifests");
        public EpicGamesLibraryService(IUserSettingsService<GeneralSettings> userSettings)
        {
            _userSettings = userSettings;
        }

        public bool TryGetInstalledGames(out List<EpicGamesInstalledGame> installedGames)
        {
            installedGames = new List<EpicGamesInstalledGame>();

            if (!TryGetInstalledGamesManifest(EpicGamesManifestPath, out var gameManifestPaths))
                return false;

            foreach (var gameManifestPath in gameManifestPaths)
            {
                var game = JsonSerializer.Deserialize<EpicGamesInstalledGame>(gameManifestPath);
                if (game is not null)
                    installedGames.Add(game);
            }

            return true;
        }

        private bool TryGetInstalledGamesManifest(string libraryPath, out List<string> installedGameManifests)
        {
            IEnumerable<string> epicAppFiles = Enumerable.Empty<string>();
            installedGameManifests = new List<string>();

            if (!Directory.Exists(libraryPath))
                return false;

            try
            {
                epicAppFiles = Directory.GetFiles(EpicGamesManifestPath);
            }
            catch
            {
                return new();
            }


            foreach (var file in epicAppFiles.ToList())
            {
                if (file.EndsWith(".item"))
                {
                    var fileContents = File.ReadAllText(file);
                    installedGameManifests.Add(fileContents);
                }
            };

            return true;
        }

        public class EpicGamesInstalledGame
        {
            [JsonPropertyName("DisplayName")]
            public string? Name { get; set; }
            [JsonPropertyName("MainGameAppName")]
            public string? AppId { get; set; }
            public string? LaunchExecutable { get; set; }
            public string? InstallLocation { get; set; }
        }
    }
}
