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
    public class LeagueLoginService : ILoginService
    {
        private string GetRiotExePath()
        {
            return @"C:\Riot Games\Riot Client\RiotClientServices.exe";
        }
        public async Task Login(Account account)
        {

            var leagueProcesses = Process.GetProcessesByName("LeagueClientUx");
            var riotProcesses = Process.GetProcessesByName("RiotClientUx");
            string queryProcess = "";
            if (leagueProcesses.Any())
                queryProcess = "LeagueClientUx.exe";
            if (riotProcesses.Any())
                queryProcess = "RiotClientUx.exe";

            if (string.IsNullOrEmpty(queryProcess))
            {
                Process.Start(@"C:\Riot Games\Riot Client\RiotClientServices.exe").WaitForInputIdle();
                queryProcess = "RiotClientUx.exe";
            }

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
            var tokenStart = wmicResponse.IndexOf("--remoting-auth-token=") + "--remoting-auth-token=".Length;
            var tokenEnd = wmicResponse.IndexOf(" ", tokenStart);
            string token = "";
            string port = "";
            if (queryProcess == "RiotClientUx.exe")
            {
                token = GetCommandLineValue(wmicResponse, "--remoting-auth-token");
                port = GetCommandLineValue(wmicResponse, "--app-port");
            }
            else if (queryProcess == "LeagueClientUx.exe")
            {
                token = GetCommandLineValue(wmicResponse, "--riotclient-auth-token");
                port = GetCommandLineValue(wmicResponse, "--riotclient-app-port");
            }

            var httpClientHandler = new HttpClientHandler();
            httpClientHandler.ServerCertificateCustomValidationCallback = (message, cert, chain, sslPolicyErrors) =>
            {
                return true;
            };

            HttpClient client = new HttpClient(httpClientHandler);
            ServicePointManager.ServerCertificateValidationCallback += (sender, cert, chain, sslPolicyErrors) => true;
            var json = new LeagueSignInRequest
            {
                Username = "***REMOVED***",
                Password = "***REMOVED***",
                PlatformId = "NA1"
            };

            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", Convert.ToBase64String(System.Text.ASCIIEncoding.ASCII.GetBytes($"riot:{token}")));
            var responseDelete = await client.DeleteAsync($"https://127.0.0.1:{port}/rso-auth/v1/authorization");
            var response = await client.PostAsJsonAsync($"https://127.0.0.1:{port}/rso-auth/v1/authorization/gas", json);
            var responseText = response.Content.ReadAsStringAsync();
        }

        public string GetCommandLineValue(string commandline , string key)
        {
            key += "=";
            var valueStart = commandline.IndexOf(key) + key.Length;
            var valueEnd = commandline.IndexOf(" ", valueStart);
            return commandline.Substring(valueStart, valueEnd - valueStart).Replace(@"\", "");
        }
    }
}
