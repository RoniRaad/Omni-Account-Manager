using AccountManager.Core.Enums;
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
    public partial class LeagueTokenClient
    {
        private readonly ITokenService _leagueTokenService;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IMapper _autoMapper;
        public LeagueTokenClient(GenericFactory<AccountType, ITokenService> tokenServiceFactory, IHttpClientFactory httpClientFactory, IMapper autoMapper)
        {
            _leagueTokenService = tokenServiceFactory.CreateImplementation(AccountType.League);
            _httpClientFactory = httpClientFactory;
            _autoMapper = autoMapper;
        }


        public async Task<string> GetLocalSessionToken()
        {
            if (!_leagueTokenService.TryGetPortAndToken(out string token, out string port))
                return string.Empty;
            var client = _httpClientFactory.CreateClient("SSLBypass");

            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", Convert.ToBase64String(System.Text.Encoding.ASCII.GetBytes($"riot:{token}")));
            var rankResponse = await client.GetFromJsonAsync<string>($"https://127.0.0.1:{port}/lol-league-session/v1/league-session-token");
            if (rankResponse is null)
                return string.Empty;

            return rankResponse;
        }

    }
}
