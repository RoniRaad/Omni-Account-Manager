using AccountManager.Core.Static;
using AccountManager.Infrastructure.Services.FileSystem;

namespace AccountManager.Core.Models
{
    public class UserSettings
    {
        public UserSettings()
        {
            SteamInstallDirectory = "";
            if (SteamFileSystemService.TryGetSteamDirectory(out var steamDirectory))
            {
                SteamInstallDirectory = steamDirectory;
            }
            SteamLibraryDirectories = new();
            
            if (!string.IsNullOrEmpty(SteamInstallDirectory))
            {
                SteamLibraryDirectories.Add(Path.Combine(SteamInstallDirectory, "appdata"));
            }

            var potentialRiotDrives = DriveInfo.GetDrives().Where((drive) => Directory.Exists($@"{drive}\Riot Games"));
            var riotDrive = potentialRiotDrives.Any() ? potentialRiotDrives.First() : null;
            if (riotDrive is not null)
                RiotInstallDirectory = Path.Combine(riotDrive.ToString(), "Riot Games");
            else
                RiotInstallDirectory = "";
        }
        public bool UseAccountCredentials { get; set; } = true;
        public string RiotInstallDirectory { get; set; }
        public string SteamInstallDirectory { get; set; }
        public List<string> SteamLibraryDirectories { get; set; }
    }
}
