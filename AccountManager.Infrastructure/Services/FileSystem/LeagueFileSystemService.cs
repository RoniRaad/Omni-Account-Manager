namespace AccountManager.Infrastructure.Services.FileSystem
{
    public class LeagueFileSystemService
    {
        public event EventHandler ClientOpened = delegate { };

        public LeagueFileSystemService()
        {
        }

        private DriveInfo? GetLeagueDrive()
        {
            return DriveInfo.GetDrives().FirstOrDefault(
                (drive) => Directory.Exists(@$"{drive?.RootDirectory}\Riot Games\League of Legends\"), null);
        }

        public string GetLeagueInstallPath()
        {
            return @$"{GetLeagueDrive()}\Riot Games\League of Legends\";
        }
    }
}
