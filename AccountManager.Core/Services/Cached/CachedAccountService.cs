using AccountManager.Core.Enums;
using AccountManager.Core.Interfaces;
using AccountManager.Core.Models;
using AccountManager.Core.Static;
using LazyCache;
using Microsoft.Extensions.Caching.Distributed;

namespace AccountManager.Core.Services.Cached
{
    public sealed class CachedAccountService : IAccountService
    {
        private const string accountCacheKey = $"{nameof(AccountService)}.accountlist";

        private readonly AccountService _accountService;
        private readonly IAppCache _memoryCache;
        private readonly IDistributedCache _persistantCache;
        public event Action OnAccountListChanged = delegate { };
        public CachedAccountService(IGenericFactory<AccountType, IPlatformService> platformServiceFactory
            , IAppCache memoryCache, IDistributedCache persistantCache, IAccountEncryptedRepository accountRepository, IAuthService authService)
        {
            _memoryCache = memoryCache;
            _persistantCache = persistantCache;
            _accountService = new(platformServiceFactory, accountRepository, authService);
            _accountService.OnAccountListChanged += () => OnAccountListChanged.Invoke();
        }

        public async Task<List<Account>> GetAllAccountsAsync()
        {
            return await _memoryCache.GetOrAddAsync(accountCacheKey, async (entry) =>
            {
                return await _accountService.GetAllAccountsAsync();
            }) ?? new();
        }

        public async Task LoginAsync(Account account)
        {
            await _persistantCache.SetAsync($"{account.Username}.riot.skip.auth", false);
            await _accountService.LoginAsync(account);
        }

        public async Task DeleteAccountAsync(Account account)
        {
            _memoryCache.Remove($"{nameof(AccountService)}.{account.Id}");
            _memoryCache.Remove(accountCacheKey);
            await _accountService.DeleteAccountAsync(account);
        }

        public async Task<Account?> GetAccountAsync(Guid id)
        {
            return await _memoryCache.GetOrAddAsync($"{nameof(AccountService)}.{id}", async (entry) =>
            {
                return await _accountService.GetAccountAsync(id);
            }) ?? new();
        }

        public async Task SaveAccountAsync(Account account)
        {
            _memoryCache.Remove(accountCacheKey);
            await _accountService.SaveAccountAsync(account);
        }
    }
}
