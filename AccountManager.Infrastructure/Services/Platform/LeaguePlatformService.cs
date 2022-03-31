using AccountManager.Core.Interfaces;
using AccountManager.Core.Models;
using System.Diagnostics;
using System.Net.Http.Json;
using AccountManager.Core.Enums;
using AccountManager.Core.Factories;
using AccountManager.Core.Models.RiotGames.League.Requests;
using AccountManager.Infrastructure.Services.FileSystem;
using AccountManager.Core.Services;
using AccountManager.Infrastructure.Clients;
using AccountManager.Core.Models.RiotGames;
using AccountManager.Core.Models.RiotGames.Valorant;
using AccountManager.Core.Models.RiotGames.Requests;

namespace AccountManager.Infrastructure.Services.Platform
{
    public class LeaguePlatformService : IPlatformService
    {
        private readonly ITokenService _riotService;
        private readonly ILeagueClient _leagueClient;
        private readonly IRiotClient _riotClient;
        private readonly HttpClient _httpClient;
        private readonly AlertService _alertService;
        private readonly RiotFileSystemService _riotFileSystemService;

        private Dictionary<string, string> RankColorMap = new Dictionary<string, string>()
        {
            {"iron", "#242424"},
            {"bronze", "#823012"},
            {"silver", "#7e878b"},
            {"gold", "#FFD700"},
            {"platinum", "#25cb6e"},
            {"diamond", "#9e7ad6"},
            {"master", "#f359f9"},
            {"grandmaster", "#f8848f"},
            {"challenger", "#4ee1ff"},
        };

        public LeaguePlatformService(ILeagueClient leagueClient, IRiotClient riotClient, GenericFactory<AccountType, ITokenService> tokenServiceFactory, 
            IHttpClientFactory httpClientFactory, RiotFileSystemService riotFileSystemService, AlertService alertService )
        {
            _leagueClient = leagueClient;
            _riotClient = riotClient;
            _riotService = tokenServiceFactory.CreateImplementation(AccountType.Valorant);
            _httpClient = httpClientFactory.CreateClient("SSLBypass");
            _riotFileSystemService = riotFileSystemService;
            _alertService = alertService;
        }

        public async Task Login(Account account)
        {
            try
            {
                foreach (var process in Process.GetProcesses())
                    if (process.ProcessName.Contains("League") || process.ProcessName.Contains("Riot"))
                        process.Kill();

                var request = new InitialAuthTokenRequest
                {
                    Id = "riot-client",
                    Nonce = "1",
                    RedirectUri = "http://localhost/redirect",
                    ResponseType = "token id_token",
                    Scope = "openid offline_access lol ban profile email phone account"
                };

                var authResponse = await _riotClient.GetRiotClientInitialCookies(request, account);
                if (authResponse.Cookies.Csid is null)
                    authResponse = await _riotClient.RiotAuthenticate(account, authResponse.Cookies);

                await _riotFileSystemService.WriteRiotYaml("NA", authResponse.Cookies.Tdid.Value, authResponse.Cookies.Ssid.Value,
                    authResponse.Cookies.Sub.Value, authResponse.Cookies.Csid.Value);

                StartLeague();
            }
            catch
            {
                _alertService.ErrorMessage = "There was an error signing in.";
            }
        }

        private void StartLeague()
        {
            var startLeagueCommandline = "--launch-product=league_of_legends --launch-patchline=live";
            var startLeague = new ProcessStartInfo
            {
                FileName = GetRiotExePath(),
                Arguments = startLeagueCommandline
            };
            Process.Start(startLeague);
        }

        public async Task<(bool, Rank)> TryFetchRank(Account account)
        {
            Rank rank = new Rank();
            try
            {
                if (string.IsNullOrEmpty(account.PlatformId))
                    account.PlatformId = await _riotClient.GetPuuId(account.Username, account.Password);
                    if (string.IsNullOrEmpty(account.PlatformId))
                        return (false, rank);

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
                if (!string.IsNullOrEmpty(account.PlatformId))
                    return (true, account.PlatformId);

                id = await _riotClient.GetPuuId(account.Username, account.Password);
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
