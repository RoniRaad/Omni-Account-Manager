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
    public class ValorantPlatformService : IPlatformService
    {
        private readonly IRiotClient _riotClient;
        private Dictionary<string, string> RankColorMap = new Dictionary<string, string>()
        {
            {"iron", "#3a3a3a"},
            {"bronze", "#816539"},
            {"silver", "#999c9b"},
            {"gold", "#e2cd5f"},
            {"platinum", "#308798"},
            {"diamond", "#f195f4"},
            {"immortal", "#ac3654"},
        };
        public ValorantPlatformService(IRiotClient riotClient)
        {
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


            var startValorantCommandline = "--launch-product=valorant --launch-patchline=live";
            var startValorant = new ProcessStartInfo
            {
                FileName = GetRiotExePath(),
                Arguments = startValorantCommandline
            };
            Process.Start(startValorant);
        }
        public string GetCommandLineValue(string commandline , string key)
        {
            key += "=";
            var valueStart = commandline.IndexOf(key) + key.Length;
            var valueEnd = commandline.IndexOf(" ", valueStart);
            return commandline.Substring(valueStart, valueEnd - valueStart).Replace(@"\", "").Replace("\"", "");
        }
        public async Task<(bool, Rank)> TryFetchRank(Account account)
        {
            Rank rank = new Rank();

            try
            {
                if (string.IsNullOrEmpty(account.Id))
                    account.Id = await _riotClient.GetPuuId(account.Username, account.Password);

                rank = await _riotClient.GetValorantRank(account);
                SetRankColor(rank);
                return new(true, rank);
            }
            catch
            {
                return new(false, rank);
            }
        }
        public async Task<(bool, string)> TryFetchId(Account account)
        {
            string id = "";

            try
            {
                if (!string.IsNullOrEmpty(account.Id))
                {
                    return new (true, account.Id);
                }

                id = await _riotClient.GetPuuId(account.Username, account.Password);
                return new(true, id);
            }
            catch
            {
                return new (false, id);
            }
        }
        private void SetRankColor(Rank rank)
        {
            foreach (KeyValuePair<string, string> kvp in RankColorMap)
                if (rank.Tier.ToLower().Equals(kvp.Key))
                    rank.Color = kvp.Value;
        }
    }
}
