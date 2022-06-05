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
using AccountManager.Core.Models.RiotGames.Valorant;
using AccountManager.Core.Models.RiotGames.Requests;
using Microsoft.Extensions.Caching.Memory;

namespace AccountManager.Infrastructure.Services.Platform
{
    public class LeaguePlatformService : IPlatformService
    {
        private readonly ITokenService _riotService;
        private readonly ILeagueClient _leagueClient;
        private readonly IRiotClient _riotClient;
        private readonly HttpClient _httpClient;
        private readonly AlertService _alertService;
        private readonly IMemoryCache _memoryCache;
        private readonly RiotFileSystemService _riotFileSystemService;
        private readonly IUserSettingsService<UserSettings> _settingsService;

        public LeaguePlatformService(ILeagueClient leagueClient, IRiotClient riotClient, GenericFactory<AccountType, 
            ITokenService> tokenServiceFactory, IHttpClientFactory httpClientFactory, RiotFileSystemService riotFileSystemService,
            AlertService alertService, IMemoryCache memoryCache, UserSettingsService<UserSettings> settingsService)
        {
            _leagueClient = leagueClient;
            _riotClient = riotClient;
            _riotService = tokenServiceFactory.CreateImplementation(AccountType.Valorant);
            _httpClient = httpClientFactory.CreateClient("SSLBypass");
            _riotFileSystemService = riotFileSystemService;
            _alertService = alertService;
            _memoryCache = memoryCache;
            _settingsService = settingsService;
        }

        private async Task<bool> TryLoginUsingRCU(Account account)
        {
            try
            {
                foreach (var process in Process.GetProcesses())
                    if (process.ProcessName.Contains("League") || process.ProcessName.Contains("Riot"))
                        process.Kill();

                await _riotFileSystemService.WaitForClientClose();
                _riotFileSystemService.DeleteLockfile();

                var startRiot = new ProcessStartInfo
                {
                    FileName = GetRiotExePath(),
                };
                Process.Start(startRiot);

                await _riotFileSystemService.WaitForClientInit();

                if (!_riotService.TryGetPortAndToken(out var token, out var port))
                    return false;

                _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", Convert.ToBase64String(System.Text.Encoding.ASCII.GetBytes($"riot:{token}")));
                await _httpClient.DeleteAsync($"https://127.0.0.1:{port}/player-session-lifecycle/v1/session");

                var lifeCycleResponse = await _httpClient.PostAsJsonAsync($"https://127.0.0.1:{port}/player-session-lifecycle/v1/session", new RiotClientApi.AuthFlowStartRequest
                {
                    LoginStrategy = "riot_identity",
                    PersistLogin = true,
                    RequireRiotID = true,
                    Scopes = new()
                    {
                        "openid",
                        "offline_access",
                        "lol",
                        "ban",
                        "profile",
                        "email",
                        "phone",
                        "account"
                    }
                });

                var resp = await _httpClient.PutAsJsonAsync($"https://127.0.0.1:{port}/rso-auth/v1/session/credentials", new RiotClientApi.LoginRequest
                {
                    Username = account.Username,
                    Password = account.Password,
                    PersistLogin = true,
                    Region = "NA1"
                });

                var credentialsResponse = await resp.Content.ReadFromJsonAsync<RiotClientApi.CredentialsResponse>();

                if (string.IsNullOrEmpty(credentialsResponse?.Type))
                {
                    _alertService.AddErrorMessage("There was an error signing in, please try again later.");
                }

                if (string.IsNullOrEmpty(credentialsResponse?.Multifactor?.Email))
                {
                    StartLeague();
                    return true;
                }

                var mfaCode = await _alertService.PromptUserFor2FA(account, credentialsResponse.Multifactor.Email);

                await _httpClient.PutAsJsonAsync($"https://127.0.0.1:{port}/rso-auth/v1/session/multifactor", new RiotClientApi.MultifactorLoginResponse
                {
                    Code = mfaCode,
                    Retry = false,
                    TrustDevice = true
                });

                StartLeague();
                return true;
            }
            catch
            {
                return false;
            }
        }

        private async Task<bool> TryLoginUsingApi(Account account)
        {
            try
            {
                foreach (var process in Process.GetProcesses())
                    if (process.ProcessName.Contains("League") || process.ProcessName.Contains("Riot") || process.ProcessName.Contains("Valorant"))
                        process.Kill();

                await _riotFileSystemService.WaitForClientClose();
                _riotFileSystemService.DeleteLockfile();

                var request = new RiotSessionRequest
                {
                    Id = "riot-client",
                    Nonce = "1",
                    RedirectUri = "http://localhost/redirect",
                    ResponseType = "token id_token",
                    Scope = "openid offline_access lol ban profile email phone account"
                };

                var authResponse = await _riotClient.RiotAuthenticate(request, account);
                if (authResponse is null || authResponse?.Cookies?.Tdid is null || authResponse?.Cookies?.Ssid is null ||
                    authResponse?.Cookies?.Sub is null || authResponse?.Cookies?.Csid is null)
                {
                    _alertService.AddErrorMessage("There was an issue authenticating with riot. We are unable to sign you in.");
                    return true;
                }

                await _riotFileSystemService.WriteRiotYaml("NA", authResponse.Cookies.Tdid.Value, authResponse.Cookies.Ssid.Value,
                    authResponse.Cookies.Sub.Value, authResponse.Cookies.Csid.Value);

                StartLeague();
                return true;
            }
            catch
            {
                return false;
            }
        }     

        public async Task Login(Account account)
        {
            if (await TryLoginUsingApi(account))
                return;
            if (await TryLoginUsingRCU(account))
                return;

            _alertService.AddErrorMessage("There was an error attempting to sign in.");
        }

        public async Task<(bool, List<RankedGraphData>)> TryFetchRankedGraphData(Account account)
        {
            var rankCacheString = $"{account.Username}.leagueoflegends.rankgraphdata";
            if (_memoryCache.TryGetValue(rankCacheString, out List <RankedGraphData> ? rankedGraphDataSets) && rankedGraphDataSets is not null)
                return (true, rankedGraphDataSets);

            var matchHistoryResponse = new UserMatchHistory();
            try
            {
                if (string.IsNullOrEmpty(account.PlatformId))
                    account.PlatformId = await _riotClient.GetPuuId(account.Username, account.Password);
                if (string.IsNullOrEmpty(account.PlatformId))
                    return (false, new());

                matchHistoryResponse = await _leagueClient.GetUserLeagueMatchHistory(account, 0, 15);
                rankedGraphDataSets = new();
                var soloQueueRank = await TryFetchRank(account);

                var matchesGroups = matchHistoryResponse?.Matches?.Reverse()?.GroupBy((match) => match.Type);
                if (matchesGroups is null)
                    return (false, new());
                
                var orderedGroups = matchesGroups.Select((group) => group.OrderBy((match) => match.EndTime));

                foreach (var matchGroup in orderedGroups)
                {
                    var matchWinDelta = 0;
                    var isFirst = true;
                    var rankedGraphData = new RankedGraphData()
                    {
                        Label = matchGroup.FirstOrDefault(new GameMatch())?.Type ?? "Other",
                        Data = new(),
                        Tags = new()
                    };
                    if (rankedGraphData.Label == "Solo")
                        rankedGraphData.ColorHex = soloQueueRank.Item2.HexColor;

                    foreach (var match in matchGroup)
                    {
                        if (!isFirst)
                        {
                                matchWinDelta += match.GraphValueChange;
                        }

                        var dateTime = match.EndTime;

                        rankedGraphData.Data.Add(new CoordinatePair() { Y = matchWinDelta, X = dateTime.ToUnixTimeMilliseconds() });
                        isFirst = false;
                    }


                    if (rankedGraphData.Data.Count > 1)
                        rankedGraphDataSets.Add(rankedGraphData);
                }

                if (matchHistoryResponse is not null)
                    _memoryCache.Set(rankCacheString, rankedGraphDataSets, TimeSpan.FromHours(1));

                if (matchHistoryResponse is null)
                    return (false, new());

                rankedGraphDataSets = rankedGraphDataSets.OrderBy((dataset) => !string.IsNullOrEmpty(dataset.ColorHex) ? 1 : 0).ToList();

                return (true, rankedGraphDataSets);
            }
            catch
            {
                return (false, new());
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
            var rankCacheString = $"{account.Username}.leagueoflegends.rank";
            if (_memoryCache.TryGetValue(rankCacheString, out Rank? rank) && rank is not null)
                return (true, rank);
            
            rank = new Rank();
            try
            {
                if (string.IsNullOrEmpty(account.PlatformId))
                    account.PlatformId = await _riotClient.GetPuuId(account.Username, account.Password);
                if (string.IsNullOrEmpty(account.PlatformId))
                    return (false, rank);

                rank = await _leagueClient.GetSummonerRankByPuuidAsync(account);

                if (!string.IsNullOrEmpty(rank?.Tier))
                    _memoryCache.Set(rankCacheString, rank, TimeSpan.FromHours(1));

                if (rank is null)
                    return (false, new Rank());

                return (true, rank);
            }
            catch
            {
                return (false, rank);
            }
        }

        public async Task<(bool, string)> TryFetchId(Account account)
        {
            try
            {
                if (!string.IsNullOrEmpty(account.PlatformId))
                    return (true, account.PlatformId);

                var id = await _riotClient.GetPuuId(account.Username, account.Password);
                return (id is not null, id ?? string.Empty);
            }
            catch
            {
                return (false, string.Empty);
            }
        }

        private string GetRiotExePath()
        {
            return @$"{_settingsService.Settings.RiotClientPath}\RiotClientServices.exe";
        }
    }
}
