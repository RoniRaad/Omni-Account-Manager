using AccountManager.Core.Models.RiotGames.Valorant.Responses;
using System.Net.Http.Json;
using Microsoft.Extensions.Logging;
using AccountManager.Infrastructure.Clients;
using AccountManager.Core.Models.RiotGames.Valorant;
using Microsoft.Extensions.Caching.Memory;
using AccountManager.Core.Models;

namespace AccountManager.Infrastructure.CachedClients
{
    public sealed class CachedRiotThirdPartyClient : IRiotThirdPartyClient
    {
        private readonly IRiotThirdPartyClient _riotThirdPartyClient;
        private readonly IMemoryCache _memoryCache;

        public CachedRiotThirdPartyClient(RiotThirdPartyClient riotThirdPartyClient, IMemoryCache memoryCache)
        {
            _riotThirdPartyClient = riotThirdPartyClient;
            _memoryCache = memoryCache;
        }

        public async Task<RiotVersionInfo?> GetRiotVersionInfoAsync()
        {
            var cacheKey = $"{nameof(GetRiotVersionInfoAsync)}";
            return await _memoryCache.GetOrCreateAsync(cacheKey,
                async (entry) =>
                {
                    return await _riotThirdPartyClient.GetRiotVersionInfoAsync();
                }) ?? new();
        }

        public async Task<ValorantOperatorsResponse> GetValorantOperators()
        {
            var cacheKey = $"{nameof(GetValorantOperators)}";
            return await _memoryCache.GetOrCreateAsync(cacheKey,
                async (entry) =>
                {
                    return await _riotThirdPartyClient.GetValorantOperators();
                }) ?? new();
        }

        public async Task<ValorantSkinLevelResponse> GetValorantSkinFromUuid(string uuid)
        {
            var cacheKey = $"{nameof(GetValorantSkinFromUuid)}.{uuid}";
            return await _memoryCache.GetOrCreateAsync(cacheKey,
                async (entry) =>
                {
                    return await _riotThirdPartyClient.GetValorantSkinFromUuid(uuid);
                }) ?? new();
        }
    }
}
