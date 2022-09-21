namespace AccountManager.Core.Models.UserSettings
{
    public class GeneralSettings
    {
        public GeneralSettings()
        {
            var potentialRiotDrives = DriveInfo.GetDrives().Where((drive) => Directory.Exists(Path.Combine(drive.ToString(), "Riot Games")));
            var potentialSteamDrives = DriveInfo.GetDrives().Where((drive) => Directory.Exists(Path.Combine(drive.ToString(), "Program Files (x86)", "Steam")));
            var potentialEpicGamesDrives = DriveInfo.GetDrives().Where((drive) => Directory.Exists(Path.Combine(drive.ToString(), "Program Files (x86)", "Epic Games")));
            var riotDrive = potentialRiotDrives.Any() ? potentialRiotDrives.First() : null;
            
            if (riotDrive is not null)
                RiotInstallDirectory = Path.Combine(riotDrive.ToString(), "Riot Games");

            if (potentialSteamDrives?.Count() > 0)
                SteamInstallDirectory = Path.Combine(potentialSteamDrives.First().ToString(),
                    "Program Files (x86)", "Steam");

            if (potentialEpicGamesDrives?.Count() > 0)
                EpicGamesInstallDirectory = Path.Combine(potentialEpicGamesDrives.First().ToString(),
                    "Program Files (x86)", "Epic Games");
        }

        public string RiotInstallDirectory { get; set; } = "";
        public string SteamInstallDirectory { get; set; } = "";
        public string EpicGamesInstallDirectory { get; set; } = "";
    }
}
