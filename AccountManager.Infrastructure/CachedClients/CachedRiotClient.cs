using AccountManager.Core.Interfaces;
using AccountManager.Core.Models;
using AccountManager.Core.Models.RiotGames;
using AccountManager.Core.Models.RiotGames.Valorant;
using AccountManager.Core.Models.RiotGames.Valorant.Responses;
using AccountManager.Core.Services;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using AccountManager.Core.Static;
using System.Text.RegularExpressions;
using AccountManager.Core.Models.RiotGames.Requests;
using AccountManager.Core.Models.AppSettings;
using Microsoft.Extensions.Options;
using AutoMapper;
using System.Web;
using AccountManager.Infrastructure.Clients;
using YamlDotNet.Core.Tokens;
using System.Security.Principal;

namespace AccountManager.Infrastructure.CachedClients
{
    public partial class CachedRiotClient : IRiotClient
    {
        private readonly IMemoryCache _memoryCache;
        private readonly IRiotClient _riotClient;
        public CachedRiotClient(IMemoryCache memoryCache,
            IRiotClient riotclient)
        {
            _memoryCache = memoryCache;
            _riotClient = riotclient;
        }

        public Task<string?> GetEntitlementToken(string token)
        {
            var cacheKey = $"{token}.{nameof(GetEntitlementToken)}";
            return _memoryCache.GetOrCreateAsync(cacheKey,
                (entry) =>
                {
                    entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(55);
                    return _riotClient.GetEntitlementToken(token);
                });
        }

        public Task<string?> GetPuuId(Account account)
        {
            var cacheKey = $"{account.Username}.{nameof(GetPuuId)}";
            return _memoryCache.GetOrCreateAsync(cacheKey,
                (entry) =>
                {
                    return _riotClient.GetPuuId(account);
                });
        }

        public Task<RiotAuthResponse?> RiotAuthenticate(RiotSessionRequest request, Account account)
        {
            return _riotClient.RiotAuthenticate(request, account);
        }

        public Task<string?> GetExpectedClientVersion()
        {
            var cacheKey = nameof(GetExpectedClientVersion);
            return _memoryCache.GetOrCreateAsync(cacheKey,
                (entry) =>
                {
                    return _riotClient.GetExpectedClientVersion();
                });
        }
    }
}
