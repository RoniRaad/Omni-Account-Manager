using AccountManager.Core.Interfaces;
using AccountManager.Core.Models;
using AccountManager.Core.Models.Steam;
using Gameloop.Vdf;
using Gameloop.Vdf.JsonConverter;
using System.Text.Json;

namespace AccountManager.Infrastructure.Services
{
    public class SteamLibraryService : ISteamLibraryService
    {
        private readonly IUserSettingsService<UserSettings> _userSettings;
        public SteamLibraryService(IUserSettingsService<UserSettings> userSettings)
        {
            _userSettings = userSettings;
        }

        public bool TryGetSteamDirectory(out string steamDirectory)
        {
            var drives = DriveInfo.GetDrives();
            var steamDrive = drives
                .FirstOrDefault((drive) => Directory.Exists(Path.Combine(drive.RootDirectory.ToString(), "Program Files (x86)", "Steam")));

            steamDirectory = "";

            if (steamDrive is null)
                return false;

            steamDirectory = Path.Combine(steamDrive.RootDirectory.ToString(), "Program Files (x86)", "Steam");

            return true;
        }

        private bool TryGetInstalledGamesManifest(string libraryPath, out List<string> installedGameManifests)
        {
            var steamLibraryPath = libraryPath;
            IEnumerable<string> steamAppFiles = Enumerable.Empty<string>();
            installedGameManifests = new List<string>();

            if (!Directory.Exists(steamLibraryPath))
                return false;

            try
            {
                steamAppFiles = Directory.GetFiles(steamLibraryPath);
            }
            catch
            {
                return new();
            }


            foreach (var file in steamAppFiles.ToList())
            {
                if (file.Contains("appmanifest"))
                {
                    var fileContents = File.ReadAllText(file);
                    installedGameManifests.Add(fileContents);
                }
            };

            return true;
        }

        public bool TryGetLibraryFolders(out LibraryFoldersWrapper? libraryFolders)
        {
            string libraryFoldersData;
            libraryFolders = null;
            try
            {
                libraryFoldersData = File.ReadAllText(Path.Combine(_userSettings.Settings.SteamInstallDirectory, "config", "libraryfolders.vdf"));
                libraryFolders = DeserializeVcfOrAcf<LibraryFoldersWrapper>(libraryFoldersData);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool TryGetGameManifests(out List<SteamGameManifest> gameManifests)
        {
            gameManifests = new List<SteamGameManifest>();

            if (!TryGetLibraryFolders(out var libraryDirectories) || libraryDirectories is null)
            {
                return false;
            }

            foreach (var folder in libraryDirectories.LibraryFolders)
            {
                if (!TryGetInstalledGamesManifest(Path.Combine(folder.Value.Path, "steamapps"), out var gameManifestData))
                {
                    return false;
                }

                try
                {
                    foreach (var gameData in gameManifestData)
                    {
            
                        gameManifests.Add(DeserializeVcfOrAcf<SteamGameManifestWrapper>(gameData).AppState);
                    }
                }
                catch
                {
                    return false;
                }
            }

            return true;
        }

        public bool TryGetUserId(Account account, out string userId)
        {
            userId = "";
            try
            {
                var userConfigData = File.ReadAllText(Path.Combine(_userSettings.Settings.SteamInstallDirectory, "config", "loginusers.vdf"));
                var users = DeserializeVcfOrAcf<SteamUsers>(userConfigData);
                var currentUser = users?.Users?.FirstOrDefault(userKvp => userKvp.Value.AccountName == account.Username);
                userId = currentUser?.Key ?? "";

                return true;
            }
            catch
            {
                return false;
            }
        }

        private T DeserializeVcfOrAcf<T>(string vcfOrAcfData) where T : new()
        {
            var deserializedManifest = VdfConvert.Deserialize(vcfOrAcfData);
            var jsonObject = deserializedManifest.ToJson();
            var deserializedObject = JsonSerializer.Deserialize<T>($"{{{jsonObject}}}", new JsonSerializerOptions() { PropertyNameCaseInsensitive = true });

            return deserializedObject ?? new();
        }
    }
}
