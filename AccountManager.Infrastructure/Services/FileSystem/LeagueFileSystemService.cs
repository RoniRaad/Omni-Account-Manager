namespace AccountManager.Infrastructure.Services.FileSystem
{
    public class LeagueFileSystemService
    {
        public event EventHandler ClientOpened = delegate { };

        public LeagueFileSystemService()
        {
            var _leagueLockFileWatcher = new FileSystemWatcher(GetLeagueInstallPath());

            _leagueLockFileWatcher.NotifyFilter = NotifyFilters.Attributes
                                 | NotifyFilters.CreationTime
                                 | NotifyFilters.DirectoryName
                                 | NotifyFilters.FileName
                                 | NotifyFilters.LastAccess
                                 | NotifyFilters.LastWrite
                                 | NotifyFilters.Security
                                 | NotifyFilters.Size;

            _leagueLockFileWatcher.EnableRaisingEvents = true;
            _leagueLockFileWatcher.Filter = "*lockfile";
            _leagueLockFileWatcher.Changed += (object sender, FileSystemEventArgs e) => ClientOpened(sender, EventArgs.Empty);
            _leagueLockFileWatcher.Created += (object sender, FileSystemEventArgs e) => ClientOpened(sender, EventArgs.Empty);
        }

        private DriveInfo? GetLeagueDrive()
        {
            return DriveInfo.GetDrives().FirstOrDefault(
                (drive) => Directory.Exists($"{drive?.RootDirectory}\\Program Files (x86)\\Steam"), null);
        }

        public string GetLeagueInstallPath()
        {
            return @$"{GetLeagueDrive()}\Riot Games\League of Legends\";
        }
    }
}
