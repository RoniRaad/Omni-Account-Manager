using AccountManager.Core.Interfaces;
using AccountManager.Core.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace AccountManager.Infrastructure.Services
{
    public class ValorantLoginService : ILoginService
    {
        private string GetRiotExePath()
        {
            return @"C:\Riot Games\Riot Client\RiotClientServices.exe";
        }
        public async Task Login(Account account)
        {
            Process? riotProcess = null;
            foreach (var process in Process.GetProcesses())
            {
                if (process.ProcessName.Contains("League") || process.ProcessName.Contains("Riot"))
                {
                    process.Kill();
                }
            }
            Process.Start(@"C:\Riot Games\Riot Client\RiotClientServices.exe");

            while (!Process.GetProcessesByName("RiotClientUx").Any())
            {
            }

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
            var wmicResponse = wmicQuery.StandardOutput.ReadToEnd();
            string token = GetCommandLineValue(wmicResponse, "--remoting-auth-token");
            string port = GetCommandLineValue(wmicResponse, "--app-port");


            var httpClientHandler = new HttpClientHandler();
            httpClientHandler.ServerCertificateCustomValidationCallback = (message, cert, chain, sslPolicyErrors) =>
            {
                return true;
            };

            HttpClient client = new HttpClient(httpClientHandler);
            var json = new LeagueSignInRequest
            {
                Username = account.Username,
                Password = account.Password,
                PlatformId = "NA1"
            };

            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", Convert.ToBase64String(System.Text.ASCIIEncoding.ASCII.GetBytes($"riot:{token}")));
            var responseDelete = await client.DeleteAsync($"https://127.0.0.1:{port}/rso-auth/v1/authorization");
            var response = await client.PostAsJsonAsync($"https://127.0.0.1:{port}/rso-auth/v1/authorization/gas", json);
            var responseText = response.Content.ReadAsStringAsync();


            var startLeagueCommandline = "--launch-product=valorant --launch-patchline=live";
            var startLeague = new ProcessStartInfo
            {
                FileName = GetRiotExePath(),
                Arguments = startLeagueCommandline
            };
            Process.Start(startLeague);
        }

        public string GetCommandLineValue(string commandline , string key)
        {
            key += "=";
            var valueStart = commandline.IndexOf(key) + key.Length;
            var valueEnd = commandline.IndexOf(" ", valueStart);
            return commandline.Substring(valueStart, valueEnd - valueStart).Replace(@"\", "").Replace("\"", "");
        }
    }
}
