﻿using AccountManager.Core.Enums;
using AccountManager.Core.Exceptions;
using AccountManager.Core.Factories;
using AccountManager.Core.Interfaces;
using AccountManager.Core.Models;
using AccountManager.Core.Models.RiotGames.Requests;
using AccountManager.Core.Models.RiotGames.Valorant;
using AccountManager.Core.Models.RiotGames.Valorant.Responses;
using AccountManager.Core.Services;
using AccountManager.Infrastructure.Services.FileSystem;
using AutoMapper;
using Microsoft.Extensions.Caching.Memory;
using System.Diagnostics;
using System.Net.Http.Json;

namespace AccountManager.Infrastructure.Services.Platform
{
    public class ValorantPlatformService : IPlatformService
    {
        private readonly ITokenService _riotService;
        private readonly IRiotClient _riotClient;
        private readonly RiotFileSystemService _riotFileSystemService;
        private readonly AlertService _alertService;
        private readonly IMemoryCache _memoryCache;
        private readonly HttpClient _httpClient;
        private readonly IMapper _mapper;
        private readonly IUserSettingsService<UserSettings> _settingsService;

        public ValorantPlatformService(IRiotClient riotClient, GenericFactory<AccountType, ITokenService> tokenServiceFactory,
            IHttpClientFactory httpClientFactory, RiotFileSystemService riotLockFileService, AlertService alertService, 
            IMemoryCache memoryCache, IMapper mapper, IUserSettingsService<UserSettings> settingsService)
        {
            _riotClient = riotClient;
            _riotService = tokenServiceFactory.CreateImplementation(AccountType.Valorant);
            _httpClient = httpClientFactory.CreateClient("SSLBypass");
            _riotFileSystemService = riotLockFileService;
            _alertService = alertService;
            _memoryCache = memoryCache;
            _mapper = mapper;
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
                    StartValorant();
                    return true;
                }

                var mfaCode = await _alertService.PromptUserFor2FA(account, credentialsResponse.Multifactor.Email);

                await _httpClient.PutAsJsonAsync($"https://127.0.0.1:{port}/rso-auth/v1/session/multifactor", new RiotClientApi.MultifactorLoginResponse
                {
                    Code = mfaCode,
                    Retry = false,
                    TrustDevice = true
                });

                StartValorant();
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

        private async Task<bool> TryLoginUsingApi(Account account)
        {
            try
            {
                foreach (var process in Process.GetProcesses())
                    if (process.ProcessName.Contains("League") || process.ProcessName.Contains("Riot"))
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

                StartValorant();
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

        private void StartValorant()
        {
            var startValorantCommandline = "--launch-product=valorant --launch-patchline=live";
            var startValorant = new ProcessStartInfo
            {
                FileName = GetRiotExePath(),
                Arguments = startValorantCommandline
            };
            Process.Start(startValorant);
        }

        public async Task<(bool, Rank)> TryFetchRank(Account account)
        {
            var rankCacheString = $"{account.Username}.valorant.rank";
            if (_memoryCache.TryGetValue(rankCacheString, out Rank? rank) && rank is not null)
                return (true, rank);

            rank = new Rank();
            try
            {
                if (string.IsNullOrEmpty(account.PlatformId))
                    account.PlatformId = await _riotClient.GetPuuId(account.Username, account.Password);
                if (string.IsNullOrEmpty(account.PlatformId))
                    return (false, rank);

                rank = await _riotClient.GetValorantRank(account);

                if (!string.IsNullOrEmpty(rank?.Tier))
                    _memoryCache.Set(rankCacheString, rank, TimeSpan.FromHours(1));

                if (rank is null)
                    return (false, new Rank());

                return new(true, rank);
            }
            catch
            {
                return new(false, new Rank());
            }
        }

        public async Task<(bool, string)> TryFetchId(Account account)
        {
            try
            {
                if (!string.IsNullOrEmpty(account.PlatformId))
                {
                    return new (true, account.PlatformId);
                }

                var id = await _riotClient.GetPuuId(account.Username, account.Password);
                return new(id is not null, id ?? string.Empty);
            }
            catch
            {
                return new (false, string.Empty);
            }
        }

        private string GetRiotExePath()
        {
            var exePath = @$"{_settingsService.Settings.RiotInstallDirectory}\Riot Client\RiotClientServices.exe";
            if (!File.Exists(exePath))
                throw new RiotClientNotFoundException();

            return exePath;
        }

        private async Task<PieChart> GetRecentlyUsedOperatorsPieChartAsync(Account account)
        {
            var matchHistory = await _riotClient.GetValorantGameHistory(account, 0, 15);
            if (matchHistory?.Any() is not true)
                return new PieChart();

            var matches = matchHistory.GroupBy((match) => _mapper.Map<ValorantCharacter>(match.Players.First((player) => player.Subject == account.PlatformId).CharacterId).Name);
            var pieChart = new PieChart();
            pieChart.Data = matches.Select((match) =>
                    {
                        return new PieChartData()
                        {
                            Value = match.Count()
                        };
                    }).ToList();

            pieChart.Labels = matches.Select((match) => match.Key).ToList();
            return pieChart;
        }


        private async Task<LineGraph> GetRankedWinsLineGraph(Account account)
        {
            var matchHistory = (await _riotClient.GetValorantGameHistory(account, 0, 15)).ToList();
            if (matchHistory?.Any() is not true)
                return new LineGraph();

            var matchesWithTierChanges = new List<ValorantMatch>();
            var currentValue = matchHistory.First().Players.First(player => player.Subject == account.PlatformId).CompetitiveTier;
            var currentIndex = 0;

            while (matchHistory.FindIndex(currentIndex, (match) => match.Players.First(player => player.Subject == account.PlatformId).CompetitiveTier != currentValue) != -1)
            {
                currentIndex = matchHistory.FindIndex(currentIndex, (match) => match.Players.First(player => player.Subject == account.PlatformId).CompetitiveTier != currentValue);
                currentValue = matchHistory.ElementAt(currentIndex).Players.First(player => player.Subject == account.PlatformId).CompetitiveTier;
                
                if (currentIndex > 1)
                    matchesWithTierChanges.Add(matchHistory.ElementAt(currentIndex - 1));

                currentIndex++;
            }

            var groupedMatches = matchHistory.GroupBy((match) =>
            {
                var rank = (int)match.Players.First(player => player.Subject == account.PlatformId).CompetitiveTier;
                return rank;
            });

            var graphData = groupedMatches.Select((match) =>
            {
                var rank = _mapper.Map<ValorantRank>(match.Key);
                var graph = new RankedGraphData();
                graph.Label = $"{rank.Tier} {rank.Ranking}";
                graph.ColorHex = ValorantRank.RankedColorMap[ValorantRank.RankMap[match.Key / 3].ToLower()];
                var yOffset = 0;
                graph.Data = match.Select((match) =>
                {
                    var teamId = match.Players.FirstOrDefault((player) => player.Subject == account.PlatformId).TeamId;
                    if (matchesWithTierChanges.Any((tierChangeMatch) => tierChangeMatch.MatchInfo.MatchId == match.MatchInfo.MatchId))
                        return new CoordinatePair()
                        {
                            Y = null,
                            X = match.MatchInfo.GameStartMillis
                        };

                    return new CoordinatePair()
                    {
                        Y = match.Teams.FirstOrDefault((team) => team.TeamId == teamId).Won ? yOffset++ : yOffset--,
                        X = match.MatchInfo.GameStartMillis
                    };
                }).ToList();

                return graph;
            });

            return new LineGraph
            {
                Data = graphData.ToList(),
                Title = "Ranked Wins"
            };
        }

        private async Task<LineGraph> GetRankedRRChangeLineGraph(Account account)
        {
            var matchHistory = await _riotClient.GetValorantCompetitiveHistory(account, 0, 15);
            if (matchHistory?.Matches?.Any() is not true)
                return new LineGraph();

            var matches = matchHistory.Matches.GroupBy((match) => match.TierAfterUpdate);
            var graphData = matches.Select((match) =>
            {
                var rank = _mapper.Map<ValorantRank>(match.Key);
                return new RankedGraphData()
                {
                    Data = match.Select((match) =>
                    {
                        return new CoordinatePair()
                        {
                            Y = match.RankedRatingAfterUpdate,
                            X = match.MatchStartTime
                        };
                    }).ToList(),
                    Tags = new(),
                    Label = $"{rank.Tier} {rank.Ranking} RR",
                    Hidden = match.Key != matchHistory.Matches.First().TierAfterUpdate,
                    ColorHex = ValorantRank.RankedColorMap[ValorantRank.RankMap[match.Key / 3].ToLower()]
                };
            }).OrderBy((graph) => graph.Hidden).ToList();

            return new LineGraph
            {
                Data = graphData,
                Title = "RR Change"
            };
        }

        public async Task<(bool, Graphs)> TryFetchRankedGraphs(Account account)
        {
            var rankedGraphs = new List<LineGraph>(); 
            rankedGraphs.Add(await GetRankedRRChangeLineGraph(account));
            rankedGraphs.Add(await GetRankedWinsLineGraph(account));

            var pieCharts = new List<PieChart>()
            {
                await GetRecentlyUsedOperatorsPieChartAsync(account) 
            };
            return (true, new Graphs { LineGraphs = rankedGraphs, PieCharts = pieCharts });
        }
    }
}
