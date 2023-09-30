using AccountManager.Core.Interfaces;
using AccountManager.Core.Models;
using AccountManager.Core.Models.RiotGames;
using Microsoft.Extensions.Caching.Memory;
using AccountManager.Core.Static;
using AccountManager.Core.Models.RiotGames.Requests;
using System.Web;
using AccountManager.Infrastructure.Clients;
using LazyCache;

namespace AccountManager.Infrastructure.CachedClients
{
    public sealed class CachedRiotTokenClient : IRiotTokenClient
    {
        private readonly IAppCache _memoryCache;
        private readonly IRiotTokenClient _riotTokenClient;
        public CachedRiotTokenClient(IAppCache memoryCache, IRiotTokenClient riotTokenClient)
        {
            _memoryCache = memoryCache;
            _riotTokenClient = riotTokenClient;
        }

        public async Task<string?> GetEntitlementToken(string accessToken)
        {
            var cacheKey = $"{accessToken}.{nameof(GetEntitlementToken)}";
            return await _memoryCache.GetOrAddAsync(cacheKey,
                async (entry) =>
                {
                    entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(55);
                    return await _riotTokenClient.GetEntitlementToken(accessToken);
                });
        }

        public async Task<string?> GetExpectedClientVersion()
        {
            var cacheKey = nameof(GetExpectedClientVersion);
            return await _memoryCache.GetOrAddAsync(cacheKey,
                async (entry) =>
                {
                    return await _riotTokenClient.GetExpectedClientVersion();
                });
        }

        public async Task<RiotAuthTokensResponse> GetRiotTokens(RiotTokenRequest request, Account account)
        {
            var cacheKey = $"{account.Username}.{request.GetHashId()}.{nameof(GetRiotTokens)}";
            return await _memoryCache.GetOrAddAsync(cacheKey,
                async (entry) =>
                {
                    var riotTokens = await _riotTokenClient.GetRiotTokens(request, account);
                    entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(riotTokens.ExpiresIn == 0 ? .1 : riotTokens.ExpiresIn - 5);
                    return riotTokens;
                }) ?? new();
        }
    }
}
