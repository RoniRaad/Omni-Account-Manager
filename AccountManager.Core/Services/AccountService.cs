using AccountManager.Core.Enums;
using AccountManager.Core.Factories;
using AccountManager.Core.Interfaces;
using AccountManager.Core.Models;
using AccountManager.Core.Services;
using AccountManager.Core.Static;
using System.Security.Principal;

namespace AccountManager.Core.Services
{
    public class AccountService : IAccountService
    {
        private IIOService _iOService;
        private AuthService _authService;
        private GenericFactory<AccountType, IPlatformService> _platformServiceFactory;
        public AccountService(IIOService iOService, AuthService authService, GenericFactory<AccountType, IPlatformService> platformServiceFactory)
        {
            _iOService = iOService;
            _authService = authService;
            _platformServiceFactory = platformServiceFactory;
        }

        public void AddAccount(Account account)
        {
            var platformService = _platformServiceFactory.CreateImplementation(account.AccountType);
            _ = Task.Run(async () =>
            {
                account.PlatformId ??= (await platformService.TryFetchId(account)).Item2;
                var rank = (await platformService.TryFetchRank(account)).Item2;
                if (!string.IsNullOrEmpty(rank.Tier))
                    account.Rank = rank;
            });
            var accounts = GetAllAccounts();
            accounts.Add(account);
            WriteAllAccounts(accounts);
        }
        public void RemoveAccount(Account account)
        {
            var accounts = GetAllAccounts();
            var relevantAccounts = accounts.Where((viewModel) => viewModel?.AccountType == account.AccountType
                && viewModel.Username == account.Username);

            if (relevantAccounts.Any())
                accounts.Remove(relevantAccounts.First());

            WriteAllAccounts(accounts);
        }
        public List<Account> GetAllAccounts()
        {
            var accounts = _iOService.ReadData<List<Account>>(_authService.PasswordHash);
            return accounts;
        }
        public void EditAccount(Account editedAccount)
        {
            var accounts = GetAllAccounts();
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
