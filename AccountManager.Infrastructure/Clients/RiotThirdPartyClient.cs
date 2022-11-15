using AccountManager.Core.Models.RiotGames.Valorant.Responses;
using System.Net.Http.Json;
using Microsoft.Extensions.Logging;
using AccountManager.Core.Models.RiotGames.Valorant;

namespace AccountManager.Infrastructure.Clients
{
    public sealed class RiotThirdPartyClient : IRiotThirdPartyClient
    {
        private readonly ILogger<RiotClient> _logger;
        private readonly IHttpClientFactory _httpClientFactory;

        public RiotThirdPartyClient(IHttpClientFactory httpClientFactory, ILogger<RiotClient> logger)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        public async Task<RiotVersionInfo?> GetRiotVersionInfoAsync()
        {
            var client = _httpClientFactory.CreateClient("Valorant3rdParty");
            try
            {
                var response = await client.GetFromJsonAsync<RiotVersionInfo>($"/v1/version");
                return response;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError("Unable to get current riot client version! Message: {Message}", ex.Message);
                throw;
            }
        }

        public async Task<ValorantOperatorsResponse> GetValorantOperators()
        {
            var client = _httpClientFactory.CreateClient("Valorant3rdParty");
            var operatorsRequest = await client.GetAsync("/v1/agents");

            try
            {
                operatorsRequest.EnsureSuccessStatusCode();
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError("Unable to get valorant operators! Status Code: {StatusCode}, Message: {Message}", ex.StatusCode, ex.Message);
                throw;
            }

            return await operatorsRequest.Content.ReadFromJsonAsync<ValorantOperatorsResponse>() ?? new();
        }

        public async Task<ValorantSkinLevelResponse> GetValorantSkinFromUuid(string uuid)
        {
            var client = _httpClientFactory.CreateClient("Valorant3rdParty");
            try
            {
                return await client.GetFromJsonAsync<ValorantSkinLevelResponse>($"/v1/weapons/skinlevels/{uuid}") ?? new();
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError("Unable to get valorant skin details for UUID: {UUID}! Status Code: {StatusCode}, Message: {Message}", uuid, ex.StatusCode, ex.Message);
                throw;
            }
        }
    }
}
