namespace AccountManager.Infrastructure.Services.FileSystem
{
    public sealed class LeagueFileSystemService
    {
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
