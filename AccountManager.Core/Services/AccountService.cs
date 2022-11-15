using AccountManager.Core.Enums;
using AccountManager.Core.Interfaces;
using AccountManager.Core.Models;

namespace AccountManager.Core.Services
{
    public sealed class AccountService : IAccountService
    {
        private readonly IGeneralFileSystemService _iOService;
        private readonly IAuthService _authService;
        private readonly IGenericFactory<AccountType, IPlatformService> _platformServiceFactory;
        public event Action OnAccountListChanged = delegate { };
        public AccountService(IGeneralFileSystemService iOService, IAuthService authService, IGenericFactory<AccountType, IPlatformService> platformServiceFactory)
        {
            _iOService = iOService;
            _authService = authService;
            _platformServiceFactory = platformServiceFactory;
        }

        public async Task RemoveAccountAsync(Account account)
        {
            var accounts = await GetAllAccountsMinAsync();
            accounts.RemoveAll((acc) => acc?.Guid == account.Guid);
            await WriteAllAccountsAsync(accounts);
            OnAccountListChanged.Invoke();
        }

        public async Task<List<Account>> GetAllAccountsAsync()
        {
            List<Task> accountTasks = new();
            var accounts = await GetAllAccountsMinAsync();

            var accountsCount = accounts.Count;
            for (int i = 0; i < accountsCount; i++)
            {
                var account = accounts[i];
                var platformService = _platformServiceFactory.CreateImplementation(account.AccountType);

                if (string.IsNullOrEmpty(account.PlatformId))
                    accountTasks.Add(Task.Run(async () => account.PlatformId = (await platformService.TryFetchId(account)).Item2));

                var updateRankTask = Task.Run(async () =>
                {
                    var rank = (await platformService.TryFetchRank(account)).Item2;
                    if (!string.IsNullOrEmpty(rank.Tier))
                        account.Rank = rank;
                });

                accountTasks.Add(updateRankTask);
            }

            await Task.WhenAll(accountTasks);

            return accounts;
        }

        public async Task<List<Account>> GetAllAccountsMinAsync()
        {
            var accounts = await _iOService.ReadDataAsync<List<Account>>(_authService.PasswordHash);
            return accounts;
        }

        public async Task LoginAsync(Account account)
        {
            var platformService = _platformServiceFactory.CreateImplementation(account.AccountType);
            await platformService.Login(account);
        }

        public async Task WriteAllAccountsAsync(List<Account> accounts)
        {
            await _iOService.WriteDataAsync(accounts, _authService.PasswordHash);
        }
    }
}
