﻿using AccountManager.Core.Enums;
using AccountManager.Core.Factories;
using AccountManager.Core.Interfaces;
using AccountManager.Core.Models;
using Microsoft.Extensions.Caching.Memory;

namespace AccountManager.Core.Services
{
    public class AccountService : IAccountService
    {
        private readonly IIOService _iOService;
        private readonly AuthService _authService;
        private readonly GenericFactory<AccountType, IPlatformService> _platformServiceFactory;
        private readonly IMemoryCache _memoryCache;
        private const string accountCacheKey = $"{nameof(AccountService)}.accountlist";
        private const string minAccountCacheKey = $"{nameof(AccountService)}.minaccountlist";

        public AccountService(IIOService iOService, AuthService authService, GenericFactory<AccountType, IPlatformService> platformServiceFactory
            , IMemoryCache memoryCache)
        {
            _iOService = iOService;
            _authService = authService;
            _platformServiceFactory = platformServiceFactory;
            _memoryCache = memoryCache;
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
                
           accounts = GetAllAccountsMin();

            var accountsCount = accounts.Count;
            for (int i = 0; i < accountsCount; i++)
            {
                var account = accounts[i];
                var platformService = _platformServiceFactory.CreateImplementation(account.AccountType);
                account.PlatformId ??= (await platformService.TryFetchId(account)).Item2;
                var rank = (await platformService.TryFetchRank(account)).Item2;
                if (!string.IsNullOrEmpty(rank.Tier))
                    account.Rank = rank;
            }

            _memoryCache.Set(accountCacheKey, accounts);

            WriteAllAccounts(accounts);
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

        public void Login(Account account)
        {
            var platformService = _platformServiceFactory.CreateImplementation(account.AccountType);
            platformService.Login(account);
        }

        public void WriteAllAccounts(List<Account> accounts)
        {
            _iOService.UpdateData(accounts, _authService.PasswordHash);
            _memoryCache.Remove(accountCacheKey);
            _memoryCache.Remove(minAccountCacheKey);
        }
    }
}
