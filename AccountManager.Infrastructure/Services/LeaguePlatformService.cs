using AccountManager.Core.Interfaces;
using AccountManager.Core.Models;
using AccountManager.Infrastructure.Clients;
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
    public class LeaguePlatformService : IPlatformService
    {
        private readonly LeagueClient _leagueClient;
        private readonly IRiotClient _riotClient;

        public LeaguePlatformService(LeagueClient leagueClient, IRiotClient riotClient)
        {
            _leagueClient = leagueClient;
            _riotClient = riotClient;
        }
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

            for (int i = 0 ; Process.GetProcessesByName("RiotClientUx").Any() && i < 3; i++) {
                System.Threading.Thread.Sleep(1000);
            }

            Process.Start(@"C:\Riot Games\Riot Client\RiotClientServices.exe");

            for (int i = 0; !Process.GetProcessesByName("RiotClientUx").Any() && i < 3; i++)
            {
                System.Threading.Thread.Sleep(1000);
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


            var startLeagueCommandline = "--launch-product=league_of_legends --launch-patchline=live";
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
        public async Task<string> TryFetchRank(Account account)
        {
            try
            {
                if (string.IsNullOrEmpty(account.Id))
                    account.Id = await _riotClient.GetPuuId(account.Username, account.Password);

                return await _leagueClient.GetRankByPuuidAsync(account.Id);
            }
            catch
            {
                return "";
            }
        }
        public async Task<string> TryFetchId(Account account)
        {
            try
            {
                if (!string.IsNullOrEmpty(account.Id))
                    return account.Id;

                return await _riotClient.GetPuuId(account.Username, account.Password);
            }
            catch
            {
                return "";
            }
        }
    }
}
