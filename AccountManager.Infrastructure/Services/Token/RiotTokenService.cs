using AccountManager.Core.Interfaces;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;

namespace AccountManager.Infrastructure.Services.Token
{
    public class RiotTokenService : ITokenService
    {
        private readonly IIOService _iOService;

        public RiotTokenService(IIOService iOService)
        {
            _iOService = iOService;
        }

        public bool TryGetPortAndToken(out string token, out string port)
        {
            port = "";
            token = "";
            var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            var fileName = $@"{appDataPath}\Riot Games\Riot Client\Config\lockfile";
            if (!_iOService.IsFileLocked(fileName))
                return false;

            using (FileStream fileStream = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            using (StreamReader fileReader = new StreamReader(fileStream))
            {
                if (!fileReader.EndOfStream)
                {
                    var lockfileContents = fileReader.ReadLine();
                    if (lockfileContents == null)
                        return false;

                    var riotParams = lockfileContents.Split(":");
                    token = riotParams[3];
                    port = riotParams[2];
                    return true;
                }
            }

            return false;
        }
    }
}
