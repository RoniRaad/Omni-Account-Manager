using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AccountManager.Core.Models
{
    public class UserSettings
    {
        public UserSettings()
        {
            var potentialRiotDrives = DriveInfo.GetDrives().Where((drive) => Directory.Exists($@"{drive}\Riot Games"));
            var riotDrive = potentialRiotDrives.Any() ? potentialRiotDrives.First() : null;
            if (riotDrive is not null)
                RiotInstallDirectory = @$"{(riotDrive)}\Riot Games\";
            else
                RiotInstallDirectory = "";
        }
        public bool UseAccountCredentials { get; set; } = true;
        public string RiotInstallDirectory { get; set; }
    }
}
