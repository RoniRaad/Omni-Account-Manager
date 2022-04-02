
namespace AccountManager.Infrastructure.Services.FileSystem
{
    public class LeagueLockFileService
    {
        private readonly FileSystemWatcher _leagueLockFileWatcher;
        public event EventHandler ClientOpened = delegate { };

        public LeagueLockFileService()
        {
            _leagueLockFileWatcher = new FileSystemWatcher(@"C:\Riot Games\League of Legends\");

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
    }
}
