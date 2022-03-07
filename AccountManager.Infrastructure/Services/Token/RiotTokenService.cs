using AccountManager.Core.Interfaces;
using System.Diagnostics;

namespace AccountManager.Infrastructure.Services.Token
{
    public class RiotTokenService : BaseRiotService, ITokenService
    {
        public bool TryGetPortAndToken(out string token, out string port)
        {
            if (!Process.GetProcessesByName("RiotClientUx").Any())
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
            var queryProcess = "RiotClientUx.exe";
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
