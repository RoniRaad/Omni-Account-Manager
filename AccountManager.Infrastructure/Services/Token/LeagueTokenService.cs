using AccountManager.Core.Interfaces;
using AccountManager.Infrastructure.Services.FileSystem;

namespace AccountManager.Infrastructure.Services.Token
{
    public sealed class LeagueTokenService : ITokenService
    {
        private readonly IIOService _iOService;
        private readonly LeagueFileSystemService _leagueFileSystemService;
        public LeagueTokenService(IIOService iOService, LeagueFileSystemService leagueFileSystemService)
        {
            _iOService = iOService;
            _leagueFileSystemService = leagueFileSystemService;
        }

        public bool TryGetPortAndToken(out string token, out string port)
        {
            port = "";
            token = "";
            var fileName = $@"{_leagueFileSystemService.GetLeagueInstallPath()}\lockfile";
            if (!_iOService.IsFileLocked(fileName))
                return false;

            using (FileStream fileStream = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            using (StreamReader fileReader = new StreamReader(fileStream))
            {
                if (!fileReader.EndOfStream)
                {
                    var leagueLockFile = fileReader.ReadLine();
                    if (string.IsNullOrEmpty(leagueLockFile))
                        return false;

                    var leagueParams = leagueLockFile.Split(":");
                    token = leagueParams[3];
                    port = leagueParams[2];
                    return true;
                }
            }

            return false;
        }
    }
}
