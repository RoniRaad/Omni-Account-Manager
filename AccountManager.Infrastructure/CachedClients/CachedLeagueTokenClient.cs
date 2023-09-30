using AccountManager.Core.Models;
using Microsoft.Extensions.Caching.Memory;
using AccountManager.Infrastructure.Clients;
using LazyCache;

namespace AccountManager.Infrastructure.CachedClients
{
    public sealed class CachedLeagueTokenClient : ILeagueTokenClient
    {
        private readonly ILeagueTokenClient _tokenClient;
        private readonly IAppCache _memoryCache;
        private static readonly SemaphoreSlim semaphore = new(1, 1);
        public CachedLeagueTokenClient(IAppCache memoryCache, ILeagueTokenClient tokenClient)
        {
            _memoryCache = memoryCache;
            _tokenClient = tokenClient;
        }

        public async Task<string> CreateLeagueSession()
        {
            return await _tokenClient.CreateLeagueSession();
        }

        public async Task<string> GetLeagueSessionToken()
        {
            var cacheKey = nameof(GetLeagueSessionToken);
            if (_memoryCache.TryGetValue(cacheKey, out string? sessionToken)
             && sessionToken is not null
             && await TestLeagueToken(sessionToken))
                return sessionToken;

            await semaphore.WaitAsync();
            try
            {
                sessionToken = await _tokenClient.GetLeagueSessionToken();

                if (!string.IsNullOrEmpty(sessionToken))
                    _memoryCache.Add(cacheKey, sessionToken);

                return sessionToken;
            }
            catch
            {
                return string.Empty;
            }
            finally
            {
                semaphore.Release(1);
            }
        }

        public async Task<string> GetLocalSessionToken()
        {
            return await _tokenClient.GetLocalSessionToken();
        }

        public async Task<bool> TestLeagueToken(string token)
        {
            return await _tokenClient.TestLeagueToken(token);
        }

        public async Task<string> GetUserInfo(Account account)
        {
            var cacheKey = $"{nameof(GetUserInfo)}.{account.Id}";
            if (_memoryCache.TryGetValue(cacheKey, out string? userInfo))
                return userInfo ?? "";

                userInfo = await _tokenClient.GetUserInfo(account);

                if (!string.IsNullOrEmpty(userInfo))
                    _memoryCache.Add(cacheKey, userInfo);

                return userInfo;
        }
    }
}
