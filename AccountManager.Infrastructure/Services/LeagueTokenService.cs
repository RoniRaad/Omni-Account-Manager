using System.Diagnostics;

namespace AccountManager.Infrastructure.Services
{
    public class LeagueTokenService : BaseRiotService
    {
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

        public string GetLeagueCommandlineParams()
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
