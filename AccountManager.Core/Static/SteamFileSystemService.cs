
using AccountManager.Core.Models.Steam;
using Gameloop.Vdf;
using Gameloop.Vdf.JsonConverter;
using Gameloop.Vdf.Linq;
using System.Text.Json;

namespace AccountManager.Core.Static
{
    public static class SteamFileSystemHelper
    {
        public static bool TryGetSteamDirectory(out string steamDirectory)
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

        public static List<string> GetInstalledGamesManifest(string libraryPath)
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

        public async static Task<SteamGameManifest> ParseGameManifest(string manifestData)
        {
            VProperty test = VdfConvert.Deserialize(manifestData);
            var deserializedObj = test.ToJson();
            var testObj = await JsonSerializer.DeserializeAsync<SteamGameManifestWrapper>(deserializedObj.ToString());
            return deserializedObj.ToObject<SteamGameManifestWrapper>().AppState;
            //return AcfDeserializer.Deserialize<SteamGameManifestWrapper>(manifestFileLines).AppState;
        }
    }
}
