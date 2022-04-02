using AccountManager.Core.Interfaces;

namespace AccountManager.Infrastructure.Services.Token
{
    public class LeagueTokenService : ITokenService
    {
        private readonly IIOService _iOService;
        public LeagueTokenService(IIOService iOService)
        {
            _iOService = iOService;
        }

        public bool TryGetPortAndToken(out string token, out string port)
        {
            port = "";
            token = "";
            var fileName = @"C:\Riot Games\League of Legends\lockfile";
            if (!_iOService.IsFileLocked(fileName))
                return false;

            using (FileStream fileStream = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            using (StreamReader fileReader = new StreamReader(fileStream))
            {
                while (!fileReader.EndOfStream)
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
