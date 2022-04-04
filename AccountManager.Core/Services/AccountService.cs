using AccountManager.Core.Enums;
using AccountManager.Core.Factories;
using AccountManager.Core.Interfaces;
using AccountManager.Core.Models;

namespace AccountManager.Core.Services
{
    public class AccountService : IAccountService
    {
        private readonly IIOService _iOService;
        private readonly AuthService _authService;
        private readonly GenericFactory<AccountType, IPlatformService> _platformServiceFactory;
        public AccountService(IIOService iOService, AuthService authService, GenericFactory<AccountType, IPlatformService> platformServiceFactory)
        {
            _iOService = iOService;
            _authService = authService;
            _platformServiceFactory = platformServiceFactory;
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
            var accounts = GetAllAccountsMin();
            var accountsCount = accounts.Count;
            for (int i = 0; i < accountsCount; i++)
            {
                var account = accounts[i];
                var platformService = _platformServiceFactory.CreateImplementation(account.AccountType);
                account.PlatformId = (await platformService.TryFetchId(account)).Item2;
                var rank = (await platformService.TryFetchRank(account)).Item2;
                if (!string.IsNullOrEmpty(rank.Tier))
                    account.Rank = rank;
            }

            return accounts;
        }

        public List<Account> GetAllAccountsMin()
        {
            var accounts = _iOService.ReadData<List<Account>>(_authService.PasswordHash);
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
        }
    }
}
