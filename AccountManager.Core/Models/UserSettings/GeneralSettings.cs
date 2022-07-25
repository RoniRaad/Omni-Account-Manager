namespace AccountManager.Core.Models.UserSettings
{
    public class GeneralSettings
    {
        public GeneralSettings()
        {
            SteamInstallDirectory = "";
            
            var potentialRiotDrives = DriveInfo.GetDrives().Where((drive) => Directory.Exists(Path.Combine(drive.ToString(), "Riot Games")));
            var potentialSteamDrives = DriveInfo.GetDrives().Where((drive) => Directory.Exists(Path.Combine(drive.ToString(), "Program Files (x86)", "Steam")));
            var riotDrive = potentialRiotDrives.Any() ? potentialRiotDrives.First() : null;
            
            if (riotDrive is not null)
                RiotInstallDirectory = Path.Combine(riotDrive.ToString(), "Riot Games");
            else
                RiotInstallDirectory = "";

            if (potentialSteamDrives?.Count() > 0)
                SteamInstallDirectory = Path.Combine(potentialSteamDrives.First().ToString(),
                    "Program Files (x86)", "Steam");
        }

        public string RiotInstallDirectory { get; set; }
        public string SteamInstallDirectory { get; set; }
    }
}
