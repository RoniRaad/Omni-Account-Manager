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

        private List<string> GetInstalledGamesManifest(string libraryPath)
        {
            var steamLibraryPath = libraryPath;
            IEnumerable<string> steamAppFiles = Enumerable.Empty<string>();
            try
            {
                steamAppFiles = Directory.GetFiles(steamLibraryPath);
            }
            catch
            {
                return new();
            }

            List<string> steamGames = new List<string>();

            steamAppFiles.ToList().ForEach((file) =>
            {
                if (file.Contains("appmanifest"))
                {
                    var fileContents = File.ReadAllText(file);
                    steamGames.Add(fileContents);
                }
            });

            return steamGames;
        }

        public LibraryFoldersWrapper GetLibraryFolders()
        {
            var libraryFoldersData = File.ReadAllText(Path.Combine(_userSettings.Settings.SteamInstallDirectory, "config", "libraryfolders.vdf"));
            return DeserializeVcfOrAcf<LibraryFoldersWrapper>(libraryFoldersData);
        }

        public List<SteamGameManifest> GetGameManifests()
        {
            var games = new List<SteamGameManifest>();
            var libraryDirectories = GetLibraryFolders();
            foreach (var folder in libraryDirectories.LibraryFolders)
            {
                var gameManifestData = GetInstalledGamesManifest(Path.Combine(folder.Value.Path, "steamapps"));
                foreach (var gameData in gameManifestData)
                {
                    try
                    {
                        games.Add(DeserializeVcfOrAcf<SteamGameManifestWrapper>(gameData).AppState);
                    }
                    catch
                    {

                    }
                }
            }

            return games;
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
