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
using AccountManager.Core.Exceptions;
using AccountManager.Core.Models.RiotGames.League;

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
            AlertService alertService, IMemoryCache memoryCache, IUserSettingsService<UserSettings> settingsService)
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
            catch (RiotClientNotFoundException)
            {
                _alertService.AddErrorMessage("Could not find riot client. Please set your riot install location in the settings page.");
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

        public async Task<(bool, Graphs)> TryFetchRankedGraphs(Account account)
        {
            var rankedGraphs = new List<LineGraph>();
            rankedGraphs.Add(await GetRankedWinsGraph(account));

            var pieCharts = new List<PieChart>();
            pieCharts.Add(await GetRankedChampSelectPieChart(account));
            
            var barCharts = new List<BarChart>();
            barCharts.Add(await GetRankedWinrateByChampBarChartAsync(account));
            barCharts.Add(await GetRankedCsRateByChampBarChartAsync(account));

            return (true, new Graphs { LineGraphs = rankedGraphs, PieCharts = pieCharts, BarCharts = barCharts });
        }

        public async Task<LineGraph> GetRankedWinsGraph(Account account)
        {
            var rankCacheString = $"{account.Username}.leagueoflegends.{nameof(GetRankedWinsGraph)}";
            if (_memoryCache.TryGetValue(rankCacheString, out LineGraph? rankedGraphDataSets) && rankedGraphDataSets is not null)
                return rankedGraphDataSets;

            try
            {
                if (string.IsNullOrEmpty(account.PlatformId))
                    account.PlatformId = await _riotClient.GetPuuId(account.Username, account.Password);
                if (string.IsNullOrEmpty(account.PlatformId))
                    return new();

                var matchHistoryResponse = await _leagueClient.GetUserLeagueMatchHistory(account, 0, 15);
                var queueMapping = await _leagueClient.GetLeagueQueueMappings();

                var userMatchHistory = new UserMatchHistory()
                {
                    Matches = matchHistoryResponse?.Games
                    ?.Where((game) => queueMapping?.FirstOrDefault((map) => map?.QueueId == game?.Json?.QueueId, null)?.Description?.Contains("Teamfights Tactics") is false)
                    ?.Select((game) =>
                    {
                        var usersTeam = game?.Json?.Participants?.FirstOrDefault((participant) => participant?.Puuid == account?.PlatformId, null)?.TeamId;

                        if (game is not null && game?.Json?.GameCreation is not null)
                            return new GameMatch()
                            {
                                Id = game?.Json?.GameId?.ToString() ?? "None",
                                GraphValueChange = game?.Json?.Teams?.FirstOrDefault((team) => team?.TeamId == usersTeam, null)?.Win ?? false ? 1 : -1,
                                EndTime = DateTimeOffset.FromUnixTimeMilliseconds(game?.Json?.GameCreation ?? 0).ToLocalTime(),
                                Type = queueMapping?.FirstOrDefault((map) => map?.QueueId == game?.Json?.QueueId, null)?.Description
                                    ?.Replace("games", "")
                                    ?.Replace("5v5", "")
                                    ?.Replace("Ranked", "")
                                    ?.Trim() ?? "Other"
                            };

                        return new();
                    })
                };

                rankedGraphDataSets = new();
                var soloQueueRank = await TryFetchRank(account);

                var matchesGroups = userMatchHistory?.Matches?.Reverse()?.GroupBy((match) => match.Type);
                if (matchesGroups is null)
                    return new();
                
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
                        rankedGraphDataSets.Data.Add(rankedGraphData);
                }

                if (userMatchHistory is not null)
                    _memoryCache.Set(rankCacheString, rankedGraphDataSets, TimeSpan.FromHours(1));

                if (userMatchHistory is null)
                    return new();

                rankedGraphDataSets.Data = rankedGraphDataSets.Data.OrderBy((dataset) => !string.IsNullOrEmpty(dataset.ColorHex) ? 1 : 0).ToList();
                rankedGraphDataSets.Title = "Ranked Wins";

                return rankedGraphDataSets;
            }
            catch
            {
                return new();
            }
        }

        public async Task<PieChart> GetRankedChampSelectPieChart(Account account)
        {
            var rankCacheString = $"{account.Username}.leagueoflegends.{nameof(GetRankedChampSelectPieChart)}";
            if (_memoryCache.TryGetValue(rankCacheString, out PieChart? rankedGraphDataSets) && rankedGraphDataSets is not null)
                return rankedGraphDataSets;

            var matchHistoryResponse = new UserChampSelectHistory();
            try
            {
                if (string.IsNullOrEmpty(account.PlatformId))
                    account.PlatformId = await _riotClient.GetPuuId(account.Username, account.Password);
                if (string.IsNullOrEmpty(account.PlatformId))
                    return new();

                matchHistoryResponse = await _leagueClient.GetUserChampSelectHistory(account, 0, 40);
                rankedGraphDataSets = new();
                if (matchHistoryResponse is not null)
                    _memoryCache.Set(rankCacheString, rankedGraphDataSets, TimeSpan.FromHours(1));

                if (matchHistoryResponse is null)
                    return new();

                rankedGraphDataSets.Data = matchHistoryResponse.Champs.Select((champs) =>
                {
                    return new PieChartData()
                    {
                        Value = champs.SelectedCount
                    };
                });
                rankedGraphDataSets.Title = "Recently Used Champs";
                rankedGraphDataSets.Labels = matchHistoryResponse.Champs.OrderByDescending((champ) => champ.SelectedCount).Select((champ) => champ.ChampName).ToList();

                return rankedGraphDataSets;
            }
            catch
            {
                return new();
            }
        }

        public async Task<BarChart> GetRankedWinrateByChampBarChartAsync(Account account)
        {
            var rankCacheString = $"{account.Username}.leagueoflegends.{nameof(GetRankedWinrateByChampBarChartAsync)}";
            if (_memoryCache.TryGetValue(rankCacheString, out BarChart? rankedGraphDataSets) && rankedGraphDataSets is not null)
                return rankedGraphDataSets;

            try
            {
                if (string.IsNullOrEmpty(account.PlatformId))
                    account.PlatformId = await _riotClient.GetPuuId(account.Username, account.Password);
                if (string.IsNullOrEmpty(account.PlatformId))
                    return new();

                var matchHistoryResponse = await _leagueClient.GetUserLeagueMatchHistory(account, 0, 40);
                rankedGraphDataSets = new();
                if (matchHistoryResponse is null)
                    return new();

                var matchesGroupedByChamp = matchHistoryResponse?.Games?.GroupBy((game) => game?.Json?.Participants?.FirstOrDefault((participant) => participant.Puuid == account.PlatformId, null)?.ChampionName);

                rankedGraphDataSets.Labels = matchesGroupedByChamp?.Select((matchGrouping) => matchGrouping.Key)?.Where((matchGrouping) => matchGrouping is not null).ToList();

                var barChartData = matchesGroupedByChamp?.Select((matchGrouping) =>
                {
                    return new BarChartData
                    {
                        Value = (double)matchGrouping.Sum((match) => match?.Json?.Participants?.FirstOrDefault((participant) => participant?.Puuid == account?.PlatformId, null)?.Win is true ? 1 : 0) / (double)matchGrouping.Count()
                    };
                });

                rankedGraphDataSets.Data = barChartData;
                rankedGraphDataSets.Title = "Recent Winrate";
                rankedGraphDataSets.Type = "percent";

                return rankedGraphDataSets;
            }
            catch
            {
                return new();
            }
        }

        public async Task<BarChart> GetRankedCsRateByChampBarChartAsync(Account account)
        {
            var rankCacheString = $"{account.Username}.leagueoflegends.{nameof(GetRankedCsRateByChampBarChartAsync)}";
            if (_memoryCache.TryGetValue(rankCacheString, out BarChart? rankedGraphDataSets) && rankedGraphDataSets is not null)
                return rankedGraphDataSets;

            try
            {
                if (string.IsNullOrEmpty(account.PlatformId))
                    account.PlatformId = await _riotClient.GetPuuId(account.Username, account.Password);
                if (string.IsNullOrEmpty(account.PlatformId))
                    return new();

                var matchHistoryResponse = await _leagueClient.GetUserLeagueMatchHistory(account, 0, 40);
                if (matchHistoryResponse is null)
                    return new();

                matchHistoryResponse.Games = matchHistoryResponse?.Games?.Where((game) => game?.Json?.GameDuration > 15 * 60).ToList();
                
                rankedGraphDataSets = new();
                var matchesGroupedByChamp = matchHistoryResponse?.Games?.GroupBy((game) => game?.Json?.Participants?.FirstOrDefault((participant) => participant.Puuid == account.PlatformId, null)?.ChampionName);
                rankedGraphDataSets.Labels = matchesGroupedByChamp?.Select((matchGrouping) => matchGrouping.Key)?.Where((matchGrouping) => matchGrouping is not null).ToList();

                var barChartData = matchesGroupedByChamp?.Select((matchGrouping) =>
                {
                    var sum = matchGrouping.Sum((match) =>
                    {
                        var minionsKilled = match?.Json?.Participants?.FirstOrDefault((participant) => participant?.Puuid == account?.PlatformId, null)?.TotalMinionsKilled;
                        var matchMinutes = match?.Json?.GameDuration / 60;
                        var minionsKilledPerMinute = minionsKilled / matchMinutes;
                        if (minionsKilled is null || matchMinutes is null)
                            return 0;

                        return minionsKilledPerMinute;
                    });

                    return new BarChartData
                    {
                        Value = (double)sum / matchGrouping.Count()
                    };
                });

                rankedGraphDataSets.Data = barChartData;
                rankedGraphDataSets.Title = "Recent CS Per Minute";

                return rankedGraphDataSets;
            }
            catch
            {
                return new();
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
            var exePath = @$"{_settingsService.Settings.RiotInstallDirectory}\Riot Client\RiotClientServices.exe";
            if (!File.Exists(exePath))
                throw new RiotClientNotFoundException();

            return exePath;
        }
    }
}
