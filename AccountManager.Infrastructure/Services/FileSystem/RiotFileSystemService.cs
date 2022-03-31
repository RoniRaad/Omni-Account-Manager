
namespace AccountManager.Infrastructure.Services.FileSystem
{
    public class RiotFileSystemService
    {
        private readonly FileSystemWatcher _riotLockFileWatcher;
        private event EventHandler clientOpened = delegate { };
        private readonly string appDataPath;
        public RiotFileSystemService()
        {
            appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
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

        private async Task<string> GenerateYaml(string region, string tdid, string ssid, string sub, string csid)
        {
            var yaml = await File.ReadAllTextAsync(@"FileTemplates\riotClientAuthTemplate.yaml");
            yaml = yaml.Replace("{region}", region);
            yaml = yaml.Replace("{tdid}", tdid);
            yaml = yaml.Replace("{ssid}", ssid);
            yaml = yaml.Replace("{sub}", sub);
            yaml = yaml.Replace("{csid}", csid);

            return yaml;
        }

        public async Task WriteRiotYaml(string region, string tdid, string ssid, string sub, string csid)
        {
            var yaml = await GenerateYaml(region, tdid, ssid, sub, csid);
            await File.WriteAllTextAsync(@$"{appDataPath}\Riot Games\Riot Client\Data\RiotGamesPrivateSettings.yaml", yaml);
        }
    }
}
