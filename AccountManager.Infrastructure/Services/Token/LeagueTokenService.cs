using AccountManager.Core.Interfaces;
using AccountManager.Core.Static;
using CloudFlareUtilities;
using Microsoft.Extensions.Caching.Memory;
using System.Diagnostics;

namespace AccountManager.Infrastructure.Services.Token
{
    public class LeagueTokenService : ITokenService
    {
        private readonly IMemoryCache _memoryCache;
        private readonly HttpClient _httpClient;
        public LeagueTokenService(IMemoryCache cache, IHttpClientFactory httpClientFactory)
        {
            _memoryCache = cache;
            _httpClient = httpClientFactory.CreateClient("CloudflareBypass");
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
            token = WmicHelper.GetCommandLineValue(leagueParams, "--remoting-auth-token");
            port = WmicHelper.GetCommandLineValue(leagueParams, "--app-port");
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
