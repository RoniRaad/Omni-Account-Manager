using AccountManager.Core.Enums;
using AccountManager.Core.Factories;
using AccountManager.Core.Interfaces;
using AccountManager.Core.Models;
using AccountManager.Core.Static;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;

namespace AccountManager.Core.Services.Cached
{
    public sealed class CachedAccountService : IAccountService
    {
        private const string accountCacheKey = $"{nameof(AccountService)}.accountlist";
        private const string minAccountCacheKey = $"{nameof(AccountService)}.minaccountlist";

        private readonly AccountService _accountService;
        private readonly IMemoryCache _memoryCache;
        private readonly IDistributedCache _persistantCache;
        private readonly SemaphoreSlim accountWriteSemaphore = new(1, 1);
        public event Action OnAccountListChanged = delegate { };
        public CachedAccountService(IIOService iOService, AuthService authService, GenericFactory<AccountType, IPlatformService> platformServiceFactory
            , IMemoryCache memoryCache, IDistributedCache persistantCache)
        {
            _memoryCache = memoryCache;
            _persistantCache = persistantCache;
            _accountService = new(iOService, authService, platformServiceFactory);
            _accountService.OnAccountListChanged += () => OnAccountListChanged.Invoke();
        }

        public void RemoveAccount(Account account)
        {
            _memoryCache.Remove(minAccountCacheKey);
            _memoryCache.Remove(accountCacheKey);
            _accountService.RemoveAccount(account);
        }

        public async Task<List<Account>> GetAllAccounts()
        {
            return await _memoryCache.GetOrCreateAsync(accountCacheKey, async (entry) =>
            {
                return await _accountService.GetAllAccounts();
            }) ?? new();
        }

        public List<Account> GetAllAccountsMin()
        {
            return _memoryCache.GetOrCreate(minAccountCacheKey, (entry) =>
            {
                return _accountService.GetAllAccountsMin();
            }) ?? new();
        }

        public async Task Login(Account account)
        {
            await _persistantCache.SetAsync($"{account.Username}.riot.skip.auth", false);
            await _accountService.Login(account);
        }

        public void WriteAllAccounts(List<Account> accounts)
        {
            accountWriteSemaphore.Wait();
            _memoryCache.Remove(accountCacheKey);
            _memoryCache.Remove(minAccountCacheKey);
            _accountService.WriteAllAccounts(accounts);
            accountWriteSemaphore.Release();
        }
    }
}
