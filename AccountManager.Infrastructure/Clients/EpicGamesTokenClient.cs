using AutoMapper;
using Microsoft.Extensions.Logging;
using System.Net.Http.Json;
using AccountManager.Core.Models;
using AccountManager.Core.Interfaces;

namespace AccountManager.Infrastructure.Clients
{
    public class EpicGamesTokenClient : IEpicGamesTokenClient
    {
        private readonly ILogger<RiotClient> _logger;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IMapper _autoMapper;

        public EpicGamesTokenClient(IHttpClientFactory httpClientFactory,
            IMapper autoMapper, ILogger<RiotClient> logger)
        {
            _httpClientFactory = httpClientFactory;
            _autoMapper = autoMapper;
            _logger = logger;
        }

        public async Task<string?> GetExchangeCode(string xsrfToken, string cookies)
        {
            HttpClient tokenExchanceClient = _httpClientFactory.CreateClient("EpicTokenExchanceApi");
            tokenExchanceClient.DefaultRequestHeaders.Add("X-XSRF-TOKEN", xsrfToken);
            tokenExchanceClient.DefaultRequestHeaders.Add("Cookie", cookies);
            var exchangeTokenResponse = await tokenExchanceClient.PostAsync("https://www.epicgames.com/id/api/exchange/generate", null);
            var code = await exchangeTokenResponse.Content.ReadFromJsonAsync<ExchangeCodeResponse>();

            return code?.ExchangeCode;
        }

        public async Task<AccessTokenResponse?> GetAccessTokenAsync(string exchangeCode)
        {
            HttpClient accountApiClient = _httpClientFactory.CreateClient("EpicAccountApi");

            accountApiClient.DefaultRequestHeaders.Clear();
            accountApiClient.DefaultRequestHeaders.Add("Authorization", "basic MzRhMDJjZjhmNDQxNGUyOWIxNTkyMTg3NmRhMzZmOWE6ZGFhZmJjY2M3Mzc3NDUwMzlkZmZlNTNkOTRmYzc2Y2Y=");
            var dict = new Dictionary<string, string>()
                            {
                                { "token_type", "eg1"},
                                { "grant_type", "exchange_code"},
                                { "exchange_code", exchangeCode},
                            };
            var content = new FormUrlEncodedContent(dict);
            var tokenResponseMessage = await accountApiClient.PostAsync("https://account-public-service-prod03.ol.epicgames.com/account/api/oauth/token", content);
            var tokenResponse = await tokenResponseMessage.Content.ReadFromJsonAsync<AccessTokenResponse>();

            return tokenResponse;
        }

        public async Task<AccountInfo?> GetAccountInfo(string accessToken, string accountId)
        {
            HttpClient accountApiClient = _httpClientFactory.CreateClient("EpicAccountApi");
            accountApiClient.DefaultRequestHeaders.Add("Authorization", $"bearer {accessToken}");
            var accountResponse = await accountApiClient.GetFromJsonAsync<AccountInfo>($"https://account-public-service-prod03.ol.epicgames.com/account/api/public/account/{accountId}");
            return accountResponse;
        }
    }
}
