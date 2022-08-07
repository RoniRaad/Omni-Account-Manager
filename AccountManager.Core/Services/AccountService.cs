using AccountManager.Core.Enums;
using AccountManager.Core.Interfaces;
using AccountManager.Core.Models;

namespace AccountManager.Core.Services
{
    public class AccountService : IAccountService
    {
        private readonly IIOService _iOService;
        private readonly IAuthService _authService;
        private readonly IGenericFactory<AccountType, IPlatformService> _platformServiceFactory;
        public event Action OnAccountListChanged = delegate { };
        public AccountService(IIOService iOService, IAuthService authService, IGenericFactory<AccountType, IPlatformService> platformServiceFactory)
        {
            _iOService = iOService;
            _authService = authService;
            _platformServiceFactory = platformServiceFactory;
        }

        public void RemoveAccount(Account account)
        {
            var accounts = GetAllAccountsMin();
            accounts.RemoveAll((acc) => acc?.Guid == account.Guid);
            WriteAllAccounts(accounts);
            OnAccountListChanged.Invoke();
        }

        public async Task<List<Account>> GetAllAccounts()
        {
            List<Task> accountTasks = new();
            var accounts = GetAllAccountsMin();

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

        public List<Account> GetAllAccountsMin()
        {
            var accounts = _iOService.ReadData<List<Account>>(_authService.PasswordHash);
            return accounts;
        }

        public async Task Login(Account account)
        {
            var platformService = _platformServiceFactory.CreateImplementation(account.AccountType);
            await platformService.Login(account);
        }

        public void WriteAllAccounts(List<Account> accounts)
        {
            _iOService.UpdateData(accounts, _authService.PasswordHash);
        }
    }
}
