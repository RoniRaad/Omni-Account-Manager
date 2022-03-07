using AccountManager.Core.Interfaces;
using CloudFlareUtilities;
using Microsoft.Extensions.Caching.Memory;
using System.Diagnostics;

namespace AccountManager.Infrastructure.Services.Token
{
    public class LeagueTokenService : BaseRiotService, ITokenService
    {
        private readonly IMemoryCache _memoryCache;
        private readonly HttpClient _httpClient;
        public LeagueTokenService(IMemoryCache cache, IHttpClientFactory httpClientFactory)
        {
            _memoryCache = cache;
            var handler = new ClearanceHandler
            {
                MaxRetries = 2 // Optionally specify the number of retries, if clearance fails (default is 3).
            };

            // TODO: Inject this client
            _httpClient = new HttpClient(handler);
        }

        public bool TryGetPortAndToken(out string token, out string port)
        {
            if (!Process.GetProcessesByName("LeagueClientUx").Any())
            {
                token = "";
                port = "";
                return false;
            }

            var leagueParams = GetLeagueCommandlineParams();
            token = GetCommandLineValue(leagueParams, "--remoting-auth-token");
            port = GetCommandLineValue(leagueParams, "--app-port");
            return true;
        }

        private string GetLeagueCommandlineParams()
        {
            var queryProcess = "LeagueClientUx.exe";
            var StartInfo = new ProcessStartInfo
            {
                FileName = "wmic",
                Arguments = $"PROCESS WHERE name='{queryProcess}' GET commandline",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                CreateNoWindow = true
            };

            var wmicQuery = Process.Start(StartInfo);
            wmicQuery.WaitForExit();
            return wmicQuery.StandardOutput.ReadToEnd();
        }
    }
}
