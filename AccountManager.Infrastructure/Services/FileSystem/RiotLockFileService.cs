
namespace AccountManager.Infrastructure.Services.FileSystem
{
    public class RiotLockFileService
    {
        private readonly FileSystemWatcher _riotLockFileWatcher;
        private event EventHandler clientOpened = delegate { };

        public RiotLockFileService()
        {
            var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            _riotLockFileWatcher = new FileSystemWatcher($@"{appDataPath}\Riot Games\Riot Client\Config\");

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
            _riotLockFileWatcher.Changed += (object sender, FileSystemEventArgs e) => clientOpened(sender, EventArgs.Empty);
            _riotLockFileWatcher.Created += (object sender, FileSystemEventArgs e) => clientOpened(sender, EventArgs.Empty);
        }

        public async Task WaitForClientInit()
        {
            EventHandler? openEvent = null;
            var clientIsOpen = false;
            openEvent = new EventHandler((args, param) =>
            {
                clientIsOpen = true;
                clientOpened -= openEvent;
            });

            clientOpened += openEvent;

            while (!clientIsOpen)
            {
                await Task.Delay(100);
            }

            return;
        }
    }
}
