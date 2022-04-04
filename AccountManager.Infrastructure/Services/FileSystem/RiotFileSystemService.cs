
using AccountManager.Core.Interfaces;

namespace AccountManager.Infrastructure.Services.FileSystem
{
    public class RiotFileSystemService
    {
        private readonly IIOService _iOService;
        private event EventHandler clientOpened = delegate { };
        private readonly string appDataPath;
        public RiotFileSystemService(IIOService iOService)
        {
            _iOService = iOService;
            appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            var riotLockFileWatcher = new FileSystemWatcher($@"{appDataPath}\Riot Games\Riot Client\Config\");

            riotLockFileWatcher.NotifyFilter = NotifyFilters.Attributes
                                 | NotifyFilters.CreationTime
                                 | NotifyFilters.DirectoryName
                                 | NotifyFilters.FileName
                                 | NotifyFilters.LastAccess
                                 | NotifyFilters.LastWrite
                                 | NotifyFilters.Security
                                 | NotifyFilters.Size;

            riotLockFileWatcher.EnableRaisingEvents = true;
            riotLockFileWatcher.Filter = "*lockfile";
            riotLockFileWatcher.Changed += (object sender, FileSystemEventArgs e) => clientOpened(sender, EventArgs.Empty);
            riotLockFileWatcher.Created += (object sender, FileSystemEventArgs e) => clientOpened(sender, EventArgs.Empty);
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
        }

        public async Task WaitForClientClose()
        {
            var lockfilePath = $@"{appDataPath}\Riot Games\Riot Client\Config\lockfile";

            var clientIsOpen = true;
            while (clientIsOpen)
            {
                clientIsOpen = _iOService.IsFileLocked(lockfilePath);
                await Task.Delay(100);
            }
        }

        public bool DeleteLockfile()
        {
            var lockfilePath = $@"{appDataPath}\Riot Games\Riot Client\Config\lockfile";
            if (File.Exists(lockfilePath))
            {
                try
                {
                    File.Delete(lockfilePath);
                    return true;
                }
                catch
                {
                    return false;
                }
            }

            return true;
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
