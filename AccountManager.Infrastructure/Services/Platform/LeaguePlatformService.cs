using AccountManager.Core.Interfaces;
using AccountManager.Core.Models;
using System.Diagnostics;
using System.Net.Http.Json;
using AccountManager.Core.Enums;
using AccountManager.Core.Factories;
using AccountManager.Core.Models.RiotGames.League.Requests;

namespace AccountManager.Infrastructure.Services.Platform
{
    public class LeaguePlatformService : IPlatformService
    {
        private readonly ITokenService _leagueService;
        private readonly ITokenService _riotService;
        private readonly ILeagueClient _leagueClient;
        private readonly IRiotClient _riotClient;
        private Dictionary<string, string> RankColorMap = new Dictionary<string, string>()
        {
            {"iron", "#372826"},
            {"bronze", "#823012"},
            {"silver", "#7e878b"},
            {"gold", "#FFD700"},
            {"platinum", "#25cb6e"},
            {"diamond", "#9e7ad6"},
            {"master", "#f359f9"},
            {"grandmaster", "#f8848f"},
            {"challenger", "#4ee1ff"},
        };
        public LeaguePlatformService(ILeagueClient leagueClient, IRiotClient riotClient, GenericFactory<AccountType, ITokenService> tokenServiceFactory)
        {
            _leagueClient = leagueClient;
            _riotClient = riotClient;
            _leagueService = tokenServiceFactory.CreateImplementation(AccountType.League);
            _riotService = tokenServiceFactory.CreateImplementation(AccountType.Valorant);
        }
        public async Task Login(Account account)
        {
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

            Process.Start(GetRiotExePath());

            for (int i = 0; !Process.GetProcessesByName("RiotClientUx").Any() && i < 3; i++)
            {
                System.Threading.Thread.Sleep(1000);
            }

            var queryProcess = "RiotClientUx.exe";
            for (int i = 0; Process.GetProcessesByName("RiotClientUx").Any() && i < 3; i++)
            {
                System.Threading.Thread.Sleep(1000);
            }
            _riotService.TryGetPortAndToken(out string token, out string port);
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
        public async Task<(bool, Rank)> TryFetchRank(Account account)
        {
            Rank rank = new Rank();
            try
            {
                if (string.IsNullOrEmpty(account.Id))
                    account.Id = await _riotClient.GetPuuId(account.Username, account.Password);

                rank = await _leagueClient.GetSummonerRankByPuuidAsync(account);
                SetRankColor(rank);
                return (true, rank);
            }
            catch
            {
                return (false, rank);
            }
        }
        public async Task<(bool, string)> TryFetchId(Account account)
        {
            var id = "";
            try
            {
                if (!string.IsNullOrEmpty(account.Id))
                    return (true, account.Id);

                id = await _leagueClient.GetPuuId(account.Username, account.Password);
                return (true, id);
            }
            catch
            {
                return (false, id);
            }
        }
        private void SetRankColor(Rank rank)
        {
            if (rank.Tier is null)
                return;

            foreach (KeyValuePair<string, string> kvp in RankColorMap)
                if (rank.Tier.ToLower().Equals(kvp.Key))
                     rank.Color = kvp.Value;
        }
        private DriveInfo FindRiotDrive()
        {
            DriveInfo riotDrive = null;
            foreach (DriveInfo drive in DriveInfo.GetDrives())
                if (Directory.Exists($"{drive.RootDirectory}\\Riot Games"))
                    riotDrive = drive;

            return riotDrive;
        }
        private string GetRiotExePath()
        {
            return @$"{FindRiotDrive().RootDirectory}\Riot Games\Riot Client\RiotClientServices.exe";
        }
    }
}
