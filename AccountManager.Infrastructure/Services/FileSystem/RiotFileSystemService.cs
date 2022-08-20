using AccountManager.Core.Interfaces;
using System.Reflection;

namespace AccountManager.Infrastructure.Services.FileSystem
{
    public class RiotFileSystemService : IRiotFileSystemService
    {
        private readonly IIOService _iOService;
        private readonly string appDataPath;
        public RiotFileSystemService(IIOService iOService)
        {
            _iOService = iOService;
            appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        }

        public async Task WaitForClientInit()
        {
            var lockfilePath = $@"{appDataPath}\Riot Games\Riot Client\Config\lockfile";

            while (!_iOService.IsFileLocked(lockfilePath))
            {
                await Task.Delay(100);
            }
        }

        public async Task WaitForClientClose()
        {
            var lockfilePath = $@"{appDataPath}\Riot Games\Riot Client\Config\lockfile";

            while (_iOService.IsFileLocked(lockfilePath))
            {
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
            string yaml;
            var assembly = Assembly.GetExecutingAssembly();
            var resources = assembly.GetManifestResourceNames();
            string resourcePath = resources
                    .Single(str => str.EndsWith("riotClientAuthTemplate.yaml"));

            using (Stream? stream = assembly?.GetManifestResourceStream(resourcePath))
            {
                if (stream is null)
                    return "";

                using StreamReader reader = new(stream);
                yaml = await reader.ReadToEndAsync();
            }

            yaml = yaml.Replace("{region}", region);
            yaml = yaml.Replace("{tdid}", tdid);
            yaml = yaml.Replace("{ssid}", ssid);
            yaml = yaml.Replace("{sub}", sub);
            yaml = yaml.Replace("{csid}", csid);

            return yaml;
        }

        public async Task WriteRiotYaml(string region, string tdid, string ssid, string sub, string csid)
        {
            var yaml = await GenerateYaml(region, tdid.Substring(tdid.IndexOf("=") + 1).Split(";")[0], ssid.Substring(tdid.IndexOf("=") + 1).Split(";")[0]
                , sub.Substring(tdid.IndexOf("=") + 1).Split(";")[0], csid.Substring(tdid.IndexOf("=") + 1).Split(";")[0]);
            await File.WriteAllTextAsync(@$"{appDataPath}\Riot Games\Riot Client\Data\RiotGamesPrivateSettings.yaml", yaml);
        }
    }
}
