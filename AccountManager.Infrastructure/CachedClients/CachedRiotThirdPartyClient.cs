using AccountManager.Core.Models.RiotGames.Valorant.Responses;
using System.Net.Http.Json;
using Microsoft.Extensions.Logging;
using AccountManager.Infrastructure.Clients;
using AccountManager.Core.Models.RiotGames.Valorant;
using Microsoft.Extensions.Caching.Memory;
using AccountManager.Core.Models;
using LazyCache;

namespace AccountManager.Infrastructure.CachedClients
{
    public sealed class CachedRiotThirdPartyClient : IRiotThirdPartyClient
    {
        private readonly IRiotThirdPartyClient _riotThirdPartyClient;
        private readonly IAppCache _memoryCache;

        public CachedRiotThirdPartyClient(RiotThirdPartyClient riotThirdPartyClient, IAppCache memoryCache)
        {
            _riotThirdPartyClient = riotThirdPartyClient;
            _memoryCache = memoryCache;
        }

        public async Task<RiotVersionInfo?> GetRiotVersionInfoAsync()
        {
            var cacheKey = $"{nameof(GetRiotVersionInfoAsync)}";
            return await _memoryCache.GetOrAddAsync(cacheKey,
                async (entry) =>
                {
                    return await _riotThirdPartyClient.GetRiotVersionInfoAsync();
                }) ?? new();
        }

        public async Task<ValorantOperatorsResponse> GetValorantOperators()
        {
            var cacheKey = $"{nameof(GetValorantOperators)}";
            return await _memoryCache.GetOrAddAsync(cacheKey,
                async (entry) =>
                {
                    return await _riotThirdPartyClient.GetValorantOperators();
                }) ?? new();
        }

        public async Task<ValorantSkinLevelResponse> GetValorantSkinFromUuid(string uuid)
        {
            var cacheKey = $"{nameof(GetValorantSkinFromUuid)}.{uuid}";
            return await _memoryCache.GetOrAddAsync(cacheKey,
                async (entry) =>
                {
                    return await _riotThirdPartyClient.GetValorantSkinFromUuid(uuid);
                }) ?? new();
        }
    }
}
