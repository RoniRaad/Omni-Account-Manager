
namespace AccountManager.Infrastructure.Services.FileSystem
{
    public class RiotLockFileService
    {
        private readonly FileSystemWatcher _riotLockFileWatcher;
        public event EventHandler ClientOpened = delegate { };

        public RiotLockFileService()
        {
            _riotLockFileWatcher = new FileSystemWatcher(@"C:\Users\Roni\AppData\Local\Riot Games\Riot Client\Config\");

            _riotLockFileWatcher.NotifyFilter = NotifyFilters.Attributes
                                 | NotifyFilters.CreationTime
                                 | NotifyFilters.DirectoryName
                                 | NotifyFilters.FileName
                                 | NotifyFilters.LastAccess
                                 | NotifyFilters.LastWrite
                                 | NotifyFilters.Security
                                 | NotifyFilters.Size;

            _riotLockFileWatcher.EnableRaisingEvents = true;
            _riotLockFileWatcher.Filter = "*lockfile";
            _riotLockFileWatcher.Changed += (object sender, FileSystemEventArgs e) => ClientOpened(sender, EventArgs.Empty);
            _riotLockFileWatcher.Created += (object sender, FileSystemEventArgs e) => ClientOpened(sender, EventArgs.Empty);
        }
    }
}
