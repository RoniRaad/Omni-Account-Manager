
using AccountManager.Core.Models.Steam;

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

        public static List<string[]> GetInstalledGamesManifest(string libraryPath)
        {
            var steamLibraryPath = libraryPath;
            string[] steamAppFiles = Directory.GetFiles(steamLibraryPath);
            List<string[]> steamGames = new List<string[]>();

            steamAppFiles.ToList().ForEach((file) =>
            {
                if (file.Contains("appmanifest"))
                {
                    string[] fileContents = File.ReadAllLines(file);
                    steamGames.Add(fileContents);
                }
            });

            return steamGames;
        }

        public static SteamGameManifest ParseGameManifest(string[] manifestFileLines)
        {
            return AcfDeserializer.Deserialize<SteamGameManifestWrapper>(manifestFileLines).AppState;
        }
    }
}
