using AccountManager.Core.Interfaces;
using AccountManager.Core.Models.EpicGames;
using AccountManager.Core.Models.UserSettings;
using System.IdentityModel.Tokens.Jwt;
using System.Text.Json;

namespace AccountManager.Infrastructure.Services
{
    public sealed class EpicGamesLibraryService : IEpicGamesLibraryService
    {
        private readonly IUserSettingsService<GeneralSettings> _userSettings;
        private readonly string EpicGamesManifestPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
            "Epic", "EpicGamesLauncher", "Data", "Manifests");
        public EpicGamesLibraryService(IUserSettingsService<GeneralSettings> userSettings)
        {
            _userSettings = userSettings;
        }

        public bool TryGetInstalledGames(out List<EpicGamesInstalledGame> installedGames)
        {
            installedGames = new List<EpicGamesInstalledGame>();

            if (!TryGetInstalledGamesManifest(EpicGamesManifestPath, out var gameManifests))
                return false;

            foreach (var gameManifest in gameManifests)
            {
                    installedGames.Add(gameManifest);
            }

            return true;
        }

        private bool TryGetInstalledGamesManifest(string libraryPath, out List<EpicGamesInstalledGame> installedGameManifests)
        {
            IEnumerable<string> epicAppFiles = Enumerable.Empty<string>();
            installedGameManifests = new List<EpicGamesInstalledGame>();

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

            installedGameManifests.AddRange(epicAppFiles.Where((file) => file.EndsWith(".item"))
                .Select((file) => JsonSerializer.Deserialize<EpicGamesInstalledGame>(File.ReadAllText(file)) ?? new())
            );

            return true;
        }
    }
}
