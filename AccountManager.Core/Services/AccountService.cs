using AccountManager.Core.Enums;
using AccountManager.Core.Factories;
using AccountManager.Core.Interfaces;
using AccountManager.Core.Models;
using AccountManager.Core.Static;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;

namespace AccountManager.Core.Services
{
    public class AccountService : IAccountService
    {
        private const string accountCacheKey = $"{nameof(AccountService)}.accountlist";
        private const string minAccountCacheKey = $"{nameof(AccountService)}.minaccountlist";

        private readonly IIOService _iOService;
        private readonly AuthService _authService;
        private readonly GenericFactory<AccountType, IPlatformService> _platformServiceFactory;
        private readonly IMemoryCache _memoryCache;
        private readonly IDistributedCache _persistantCache;
        private readonly SemaphoreSlim accountWriteSemaphore = new SemaphoreSlim(1, 1);

        public AccountService(IIOService iOService, AuthService authService, GenericFactory<AccountType, IPlatformService> platformServiceFactory
            , IMemoryCache memoryCache, IDistributedCache persistantCache)
        {
            _iOService = iOService;
            _authService = authService;
            _platformServiceFactory = platformServiceFactory;
            _memoryCache = memoryCache;
            _persistantCache = persistantCache;
        }

        public void AddAccount(Account account)
        {
            var accounts = GetAllAccountsMin();
            accounts.Add(account);
            WriteAllAccounts(accounts);
        }

        public void RemoveAccount(Account account)
        {
            var accounts = GetAllAccountsMin();
            accounts.RemoveAll((acc) => acc?.Guid == account.Guid);

            WriteAllAccounts(accounts);
        }

        public async Task<List<Account>> GetAllAccounts()
        {
            if (_memoryCache.TryGetValue<List<Account>>(accountCacheKey, out var accounts) && accounts is not null)
                return accounts;

            List<Task> accountTasks = new();
            accounts = GetAllAccountsMin();

            var accountsCount = accounts.Count;
            for (int i = 0; i < accountsCount; i++)
            {
                var account = accounts[i];
                var platformService = _platformServiceFactory.CreateImplementation(account.AccountType);
                if (string.IsNullOrEmpty(account.PlatformId))
                {
                    accountTasks.Add(Task.Run(async () => account.PlatformId = (await platformService.TryFetchId(account)).Item2));
                }

                var updateRankTask = Task.Run(async () =>
                {
                    var rank = (await platformService.TryFetchRank(account)).Item2;
                    if (!string.IsNullOrEmpty(rank.Tier))
                        account.Rank = rank;
                });

                accountTasks.Add(updateRankTask);
            }

            await Task.WhenAll(accountTasks);

            WriteAllAccounts(accounts);
            _memoryCache.Set(accountCacheKey, accounts);

            return accounts;
        }

        public List<Account> GetAllAccountsMin()
        {
            if (!_memoryCache.TryGetValue(minAccountCacheKey, out List<Account>? accounts) && accounts is not null)
                return accounts;

            accounts = _iOService.ReadData<List<Account>>(_authService.PasswordHash);

            _memoryCache.Set(minAccountCacheKey, accounts);

            return accounts;
        }

        public void EditAccount(Account editedAccount)
        {
            var accounts = GetAllAccountsMin();
            accounts.ForEach(account =>
            {
                if (account.Guid == editedAccount.Guid)
                {
                    account.Username = editedAccount.Username;
                    account.Password = editedAccount.Password;
                    account.AccountType = editedAccount.AccountType;
                    account.Id = editedAccount.Id;
                    account.PlatformId = editedAccount.PlatformId;
                }
            });

            WriteAllAccounts(accounts);
        }

        public async Task Login(Account account)
        {
            await _persistantCache.SetAsync($"{account.Username}.riot.skip.auth", false);
            var platformService = _platformServiceFactory.CreateImplementation(account.AccountType);
            await platformService.Login(account);
        }

        public void WriteAllAccounts(List<Account> accounts)
        {
            accountWriteSemaphore.Wait();
            _iOService.UpdateData(accounts, _authService.PasswordHash);
            _memoryCache.Remove(accountCacheKey);
            _memoryCache.Remove(minAccountCacheKey);
            accountWriteSemaphore.Release();
        }
    }
}
