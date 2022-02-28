using AccountManager.Infrastructure.Services;
using AccountManager.Infrastructure.Services.RankingServices;
using System.Net.Http.Json;
using System.Text.Json.Serialization;

namespace AccountManager.Infrastructure.Clients
{
    public class LeagueClient
    {
        private string token;
        private string port;
        private readonly LeagueTokenService _leagueTokenService;
        public LeagueClient(LeagueTokenService leagueTokenService)
        {
            _leagueTokenService = leagueTokenService;
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
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", Convert.ToBase64String(System.Text.ASCIIEncoding.ASCII.GetBytes($"riot:{token}")));
            var response = await client.GetAsync($"https://127.0.0.1:{port}/lol-summoner/v1/summoners?name={username}");
            var summoner = await response.Content.ReadFromJsonAsync<LeagueAccount>();
            var rankResponse = await client.GetAsync($"https://127.0.0.1:{port}/lol-ranked/v1/ranked-stats/{summoner.Puuid}");
            var summonerRanking = await rankResponse.Content.ReadFromJsonAsync<LeagueSummonerRank>();
            return $"{summonerRanking.QueueMap.RankedSoloDuoStats.Tier} {summonerRanking.QueueMap.RankedSoloDuoStats.Division}";
        }

        public async Task<string> GetRankByPuuidAsync(string puuid)
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
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", Convert.ToBase64String(System.Text.ASCIIEncoding.ASCII.GetBytes($"riot:{token}")));
            var rankResponse = await client.GetAsync($"https://127.0.0.1:{port}/lol-ranked/v1/ranked-stats/{puuid}");
            var summonerRanking = await rankResponse.Content.ReadFromJsonAsync<LeagueSummonerRank>();
            return $"{summonerRanking.QueueMap.RankedSoloDuoStats.Tier} {summonerRanking.QueueMap.RankedSoloDuoStats.Division}";
        }

        public async Task<LeagueAccount> GetSignedInAccountAsync()
        {
            if (!_leagueTokenService.TryGetPortAndToken(out token, out port))
                return null;

            var httpClientHandler = new HttpClientHandler();
            httpClientHandler.ServerCertificateCustomValidationCallback = (message, cert, chain, sslPolicyErrors) =>
            {
                return true;
            };

            // TODO: Inject this client
            HttpClient client = new HttpClient(httpClientHandler);
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", Convert.ToBase64String(System.Text.ASCIIEncoding.ASCII.GetBytes($"riot:{token}")));
            var response = await client.GetAsync($"https://127.0.0.1:{port}/lol-login/v1/session");
            var summoner = await response.Content.ReadFromJsonAsync<LeagueAccount>();
            return summoner;
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
