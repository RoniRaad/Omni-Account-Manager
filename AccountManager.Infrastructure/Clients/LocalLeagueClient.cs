using AccountManager.Core.Enums;
using AccountManager.Core.Factories;
using AccountManager.Core.Interfaces;
using AccountManager.Core.Models;
using AccountManager.Core.Models.RiotGames.League;
using System.Net.Http.Json;
using System.Text.Json.Serialization;

namespace AccountManager.Infrastructure.Clients
{
    public class LocalLeagueClient : ILeagueClient
    {
        private readonly ITokenService _leagueTokenService;
        private readonly HttpClient _httpClient;
        public LocalLeagueClient(GenericFactory<AccountType, ITokenService> tokenServiceFactory, IHttpClientFactory httpClientFactory)
        {
            _leagueTokenService = tokenServiceFactory.CreateImplementation(AccountType.League);
            _httpClient = httpClientFactory.CreateClient("SSLBypass");
        }

        public bool IsClientOpen()
        {
            return _leagueTokenService.TryGetPortAndToken(out string token, out string port);
        }

        public async Task<string> GetLocalSessionToken()
        {
            if (!_leagueTokenService.TryGetPortAndToken(out string token, out string port))
                return string.Empty;

            _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", Convert.ToBase64String(System.Text.Encoding.ASCII.GetBytes($"riot:{token}")));
            var rankResponse = await _httpClient.GetFromJsonAsync<LeagueSessionResponse>($"https://127.0.0.1:{port}/lol-login/v2/league-session-init-token");
            return rankResponse.Token;
        }
        public class LeagueSessionResponse
        {
            [JsonPropertyName("token")]
            public string Token { get; set; }
        }
        public async Task<string> GetRankByUsernameAsync(string username)
        {
            if (!_leagueTokenService.TryGetPortAndToken(out string token, out string port))
                return "";

            _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", Convert.ToBase64String(System.Text.Encoding.ASCII.GetBytes($"riot:{token}")));
            var response = await _httpClient.GetAsync($"https://127.0.0.1:{port}/lol-summoner/v1/summoners?name={username}");
            var summoner = await response.Content.ReadFromJsonAsync<LeagueAccount>();
            var rankResponse = await _httpClient.GetAsync($"https://127.0.0.1:{port}/lol-ranked/v1/ranked-stats/{summoner.Puuid}");
            var summonerRanking = await rankResponse.Content.ReadFromJsonAsync<LeagueSummonerRank>();
            return $"{summonerRanking.QueueMap.RankedSoloDuoStats.Tier} {summonerRanking.QueueMap.RankedSoloDuoStats.Division}";
        }
        public async Task<QueueMap> GetRankQueuesByPuuidAsync(Account account)
        {
            if (!_leagueTokenService.TryGetPortAndToken(out string token, out string port))
                return null;

            _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", Convert.ToBase64String(System.Text.ASCIIEncoding.ASCII.GetBytes($"riot:{token}")));
            var rankResponse = await _httpClient.GetAsync($"https://127.0.0.1:{port}/lol-ranked/v1/ranked-stats/{account.Id}");
            var summonerRanking = await rankResponse.Content.ReadFromJsonAsync<LeagueSummonerRank>();

            return summonerRanking.QueueMap;
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

            return rank;
        }        
        public async Task<Rank> GetTFTRankByPuuidAsync(Account account)
        {
            if (!_leagueTokenService.TryGetPortAndToken(out string token, out string port))
                return new Rank();

            var queueMap = await GetRankQueuesByPuuidAsync(account);
            if (queueMap.TFTStats.Tier.ToLower() == "none")
                return new Rank()
                {
                    Tier = "UNRANKED",
                    Ranking = ""
                };

            var rank = new Rank()
            {
                Tier = queueMap?.TFTStats?.Tier,
                Ranking = queueMap?.TFTStats?.Division,
            };

            return rank;
        }

        public Task<string> GetPuuId(string username, string password)
        {
            throw new NotImplementedException();
        }
    }

    public class LeagueAccount
    {
        [JsonPropertyName("puuid")]
        public string Puuid { get; set; }
        [JsonPropertyName("username")]
        public string Username { get; set; }
    }
}
