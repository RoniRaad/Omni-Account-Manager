using AccountManager.Core.Enums;
using AccountManager.Core.Factories;
using AccountManager.Core.Interfaces;
using AccountManager.Core.Models;
using AccountManager.Core.Models.RiotGames.League.Requests;
using System.Diagnostics;
using System.Net.Http.Json;

namespace AccountManager.Infrastructure.Services.Platform
{
    public class ValorantPlatformService : IPlatformService
    {
        private readonly ITokenService _riotService;
        private readonly IRiotClient _riotClient;
        private readonly HttpClient _httpClient;
        private Dictionary<string, string> RankColorMap = new Dictionary<string, string>()
        {
            {"iron", "#3a3a3a"},
            {"bronze", "#823012"},
            {"silver", "#999c9b"},
            {"gold", "#e2cd5f"},
            {"platinum", "#308798"},
            {"diamond", "#f195f4"},
            {"immortal", "#ac3654"},
        };
        public ValorantPlatformService(IRiotClient riotClient, GenericFactory<AccountType, ITokenService> tokenServiceFactory, IHttpClientFactory httpClientFactory)
        {
            _riotClient = riotClient;
            _riotService = tokenServiceFactory.CreateImplementation(AccountType.Valorant);
            _httpClient = httpClientFactory.CreateClient("SSLBypass");

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

            for (int i = 0; Process.GetProcessesByName("RiotClientUx").Any() && i < 3; i++)
            {
                System.Threading.Thread.Sleep(1000);
            }

            Process.Start(GetRiotExePath());

            for (int i = 0; !Process.GetProcessesByName("RiotClientUx").Any() && i < 3; i++)
            {
                System.Threading.Thread.Sleep(1000);
            }

            for (int i = 0; Process.GetProcessesByName("RiotClientUx").Any() && i < 3; i++)
            {
                System.Threading.Thread.Sleep(1000);
            }
            _riotService.TryGetPortAndToken(out string token, out string port);

            var signInRequest = new LeagueSignInRequest
            {
                Username = account.Username,
                Password = account.Password,
                PlatformId = "NA1"
            };

            _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", Convert.ToBase64String(System.Text.Encoding.ASCII.GetBytes($"riot:{token}")));
            var responseDelete = await _httpClient.DeleteAsync($"https://127.0.0.1:{port}/rso-auth/v1/authorization");
            var response = await _httpClient.PostAsJsonAsync($"https://127.0.0.1:{port}/rso-auth/v1/authorization/gas", signInRequest);

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
