using AccountManager.Core.Enums;
using AccountManager.Core.Factories;
using AccountManager.Core.Interfaces;
using AccountManager.Core.Models;
using AccountManager.Core.Models.League;
using AccountManager.Infrastructure.Services;
using System;
using System.Net.Http.Json;
using System.Text.Json.Serialization;

namespace AccountManager.Infrastructure.Clients
{
    public class LocalLeagueClient : ILeagueClient
    {
        private string token;
        private string port;
        private readonly ITokenService _leagueTokenService;
        public LocalLeagueClient(GenericFactory<AccountType, ITokenService> tokenServiceFactory)
        {
            _leagueTokenService = tokenServiceFactory.CreateImplementation(AccountType.League);
        }

        public bool IsClientOpen()
        {
            return _leagueTokenService.TryGetPortAndToken(out token, out port);
        }

        public async Task<string> GetLocalSessionToken()
        {
            if (!_leagueTokenService.TryGetPortAndToken(out token, out port))
                return string.Empty;

            var httpClientHandler = new HttpClientHandler();
            httpClientHandler.ServerCertificateCustomValidationCallback = (message, cert, chain, sslPolicyErrors) =>
            {
                return true;
            };

            // TODO: Inject this client
            HttpClient client = new HttpClient(httpClientHandler);
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", Convert.ToBase64String(System.Text.Encoding.ASCII.GetBytes($"riot:{token}")));
            var rankResponse = await client.GetFromJsonAsync<LeagueSessionResponse>($"https://127.0.0.1:{port}/lol-login/v1/session");
            return rankResponse.Token;
        }
        public class LeagueSessionResponse
        {
            [JsonPropertyName("token")]
            public string Token { get; set; }
        }
        public async Task<string> GetRankByUsernameAsync(string username)
        {
            if (!_leagueTokenService.TryGetPortAndToken(out token, out port))
                return "";

            var httpClientHandler = new HttpClientHandler();
            httpClientHandler.ServerCertificateCustomValidationCallback = (message, cert, chain, sslPolicyErrors) =>
            {
                return true;
            };

            // TODO: Inject this client
            HttpClient client = new HttpClient(httpClientHandler);
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", Convert.ToBase64String(System.Text.Encoding.ASCII.GetBytes($"riot:{token}")));
            var response = await client.GetAsync($"https://127.0.0.1:{port}/lol-summoner/v1/summoners?name={username}");
            var summoner = await response.Content.ReadFromJsonAsync<LeagueAccount>();
            var rankResponse = await client.GetAsync($"https://127.0.0.1:{port}/lol-ranked/v1/ranked-stats/{summoner.Puuid}");
            var summonerRanking = await rankResponse.Content.ReadFromJsonAsync<LeagueSummonerRank>();
            return $"{summonerRanking.QueueMap.RankedSoloDuoStats.Tier} {summonerRanking.QueueMap.RankedSoloDuoStats.Division}";
        }

        public async Task<Rank> GetRankByPuuidAsync(Account account)
        {
            if (!_leagueTokenService.TryGetPortAndToken(out token, out port))
                return new Rank();

            var httpClientHandler = new HttpClientHandler();
            httpClientHandler.ServerCertificateCustomValidationCallback = (message, cert, chain, sslPolicyErrors) =>
            {
                return true;
            };

            // TODO: Inject this client
            HttpClient client = new HttpClient(httpClientHandler);
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", Convert.ToBase64String(System.Text.ASCIIEncoding.ASCII.GetBytes($"riot:{token}")));
            var rankResponse = await client.GetAsync($"https://127.0.0.1:{port}/lol-ranked/v1/ranked-stats/{account.Id}");
            var str = rankResponse.Content.ReadAsStringAsync();
            var summonerRanking = await rankResponse.Content.ReadFromJsonAsync<LeagueSummonerRank>();
            var rank = new Rank()
            {
                Tier = summonerRanking?.QueueMap?.RankedSoloDuoStats?.Tier,
                Ranking = summonerRanking?.QueueMap?.RankedSoloDuoStats?.Division,
            };

            return rank;
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
