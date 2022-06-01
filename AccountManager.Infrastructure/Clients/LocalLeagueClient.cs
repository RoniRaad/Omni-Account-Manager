﻿using AccountManager.Core.Enums;
using AccountManager.Core.Factories;
using AccountManager.Core.Interfaces;
using AccountManager.Core.Models;
using AccountManager.Core.Models.RiotGames.League;
using System.Net.Http.Json;
using AccountManager.Core.Models.RiotGames.League.Responses;
using System.Net.Http;
using AccountManager.Core.Models.RiotGames.League.Requests;
using AutoMapper;
using AccountManager.Core.Models.RiotGames.TeamFightTactics.Responses;

namespace AccountManager.Infrastructure.Clients
{
    public partial class LocalLeagueClient
    {
        private readonly ITokenService _leagueTokenService;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IMapper _autoMapper;
        public LocalLeagueClient(GenericFactory<AccountType, ITokenService> tokenServiceFactory, IHttpClientFactory httpClientFactory, IMapper autoMapper)
        {
            _leagueTokenService = tokenServiceFactory.CreateImplementation(AccountType.League);
            _httpClientFactory = httpClientFactory;
            _autoMapper = autoMapper;
        }

        public bool IsClientOpen() =>
             _leagueTokenService.TryGetPortAndToken(out string token, out string port);

        public async Task<string> GetLocalSessionToken()
        {
            if (!_leagueTokenService.TryGetPortAndToken(out string token, out string port))
                return string.Empty;
            var client = _httpClientFactory.CreateClient("SSLBypass");

            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", Convert.ToBase64String(System.Text.Encoding.ASCII.GetBytes($"riot:{token}")));
            var rankResponse = await client.GetFromJsonAsync<LeagueSessionResponse>($"https://127.0.0.1:{port}/lol-login/v2/league-session-init-token");
            if (rankResponse?.Token is null)
                return string.Empty;

            return rankResponse.Token;
        }

        public async Task<QueueMap?> GetRankQueuesByPuuidAsync(Account account)
        {
            if (!_leagueTokenService.TryGetPortAndToken(out string token, out string port))
                return null;

            var client = _httpClientFactory.CreateClient("SSLBypass");

            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", Convert.ToBase64String(System.Text.Encoding.ASCII.GetBytes($"riot:{token}")));
            var rankResponse = await client.GetAsync($"https://127.0.0.1:{port}/lol-ranked/v1/ranked-stats/{account.PlatformId}");
            var summonerRanking = await rankResponse.Content.ReadFromJsonAsync<LeagueSummonerRank>();

            return summonerRanking?.QueueMap;
        }

        public async Task<Rank> GetSummonerRankByPuuidAsync(Account account)
        {
            if (!_leagueTokenService.TryGetPortAndToken(out string token, out string port))
                return new Rank();


            var queueMap = await GetRankQueuesByPuuidAsync(account);
            var rank = new Rank()
            {
                Tier = queueMap?.RankedSoloDuoStats?.Tier,
                Ranking = queueMap?.RankedSoloDuoStats?.Division,
            };

            return _autoMapper.Map<LeagueRank>(rank);
        }        

        public async Task<Rank> GetTFTRankByPuuidAsync(Account account)
        {
            if (!_leagueTokenService.TryGetPortAndToken(out _, out _))
                return new Rank();

            var queueMap = await GetRankQueuesByPuuidAsync(account);
            if (queueMap?.TFTStats?.Tier?.ToLower() == "none")
                return _autoMapper.Map<TeamFightTacticsRank>(new Rank()
                {
                    Tier = "UNRANKED",
                    Ranking = ""
                });

            var rank = new Rank()
            {
                Tier = queueMap?.TFTStats?.Tier,
                Ranking = queueMap?.TFTStats?.Division,
            };

            return _autoMapper.Map<TeamFightTacticsRank>(rank);
        }

        public async Task<List<LeagueQueueMapResponse>?> GetLeagueQueueMappings()
        {
            var client = _httpClientFactory.CreateClient();
            var response = await client.GetFromJsonAsync<List<LeagueQueueMapResponse>>("https://static.developer.riotgames.com/docs/lol/queues.json");

            return response;
        }

        public async Task<UserMatchHistory?> GetUserMatchHistory(Account account, int startIndex, int endIndex)
        {
            if (!_leagueTokenService.TryGetPortAndToken(out string token, out string port))
                return null;

            var client = _httpClientFactory.CreateClient("SSLBypass");

            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", Convert.ToBase64String(System.Text.Encoding.ASCII.GetBytes($"riot:{token}")));
            var rankResponse = await client.GetFromJsonAsync<LocalLeagueMatchHistoryResponse>($"https://127.0.0.1:{port}/lol-match-history/v1/products/lol/{account.PlatformId}/matches?begIndex={startIndex}&endIndex={endIndex}");

            if (rankResponse is null)
                return new();

            var queueMapping = await GetLeagueQueueMappings();

            var matchHistory = new UserMatchHistory()
            {
                Matches = rankResponse.Games.Games
                .Where((game) => queueMapping?.FirstOrDefault((map) => map?.QueueId == game.QueueId, null)?.Description?.Contains("Teamfights Tactics") is false)
                .Select((game) =>
                {
                    var usersTeam = game.Participants[0].TeamId;

                    if (game is not null && game?.GameCreation is not null)
                        return new GameMatch()
                        {
                            Id = game?.GameId?.ToString() ?? "None",
                            GraphValueChange = game?.Teams?.FirstOrDefault((team) => team?.TeamId == usersTeam, null)?.Win?.ToLower()?.Equals("win") ?? false ? 1 : -1,
                            EndTime = DateTimeOffset.FromUnixTimeMilliseconds(game?.GameCreation ?? 0).ToLocalTime(),
                            Type = queueMapping?.FirstOrDefault((map) => map?.QueueId == game?.QueueId, null)?.Description
                                ?.Replace("games", "")
                                ?.Replace("5v5", "")
                                ?.Replace("Ranked", "")
                                ?.Trim() ?? "Other"
                        };

                    return new();
                })
            };

            return matchHistory;
        }

        public async Task<UserMatchHistory?> GetUserTeamFightTacticsMatchHistory(Account account, int startIndex, int endIndex)
        {
            if (!_leagueTokenService.TryGetPortAndToken(out string token, out string port))
                return null;

            var client = _httpClientFactory.CreateClient("SSLBypass");

            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", Convert.ToBase64String(System.Text.Encoding.ASCII.GetBytes($"riot:{token}")));
            var rankHttpResponse = await client.GetAsync($"https://127.0.0.1:{port}/lol-match-history/v1/products/tft/{account.PlatformId}/matches?begin={startIndex}&count={endIndex}");
            var rankResponse = await rankHttpResponse.Content.ReadFromJsonAsync<TeamFightTacticsMatchHistoryResponse>();
            if (rankResponse is null)
                return new();

            var queueMapping = await GetLeagueQueueMappings();

            var matchHistory = new UserMatchHistory()
            {
                Matches = rankResponse?.Games
                ?.Select((game) =>
                {
                    if (game is not null && game?.Metadata?.Timestamp is not null)
                        return new GameMatch()
                        {
                            Id = game?.Json?.GameId?.ToString() ?? "None",
                            // 4th place grants no value while going up and down adds 1 positive and negative value for each movement
                            GraphValueChange = (game?.Json?.Participants?.First((participant) => participant.Puuid == account.PlatformId)?.Placement - 4) * -1 ?? 0,
                            EndTime = DateTimeOffset.FromUnixTimeMilliseconds(game?.Metadata?.Timestamp ?? 0).ToLocalTime(),
                            Type = queueMapping?.FirstOrDefault((map) => map?.QueueId == game?.Json?.QueueId, null)?.Description
                                ?.Replace("games", "")
                                ?.Replace("5v5", "")
                                ?.Replace("Ranked", "")
                                ?.Trim() ?? "Other"
                        };

                    return new();
                })
            };

            return matchHistory;
        }
    }
}
