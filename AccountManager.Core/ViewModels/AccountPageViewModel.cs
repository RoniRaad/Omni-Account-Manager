using AccountManager.Core.Enums;
using AccountManager.Core.Factories;
using AccountManager.Core.Interfaces;
using AccountManager.Core.Models;
using AccountManager.Core.Services;
using AccountManager.Core.Static;
using System.Security.Principal;

namespace AccountManager.Core.ViewModels
{
    public class AccountPageViewModel
    {
        public List<AccountListItemViewModel> AccountLists = new List<AccountListItemViewModel>();
        public AccountListItemViewModel NewAccount { get; set; } = new AccountListItemViewModel();
        public Action UpdateView { get; set; }
        public bool AddAccountPromptShow = false;
        private IIOService _iOService;
        private AuthService _authService;
        private GenericFactory<AccountType, IPlatformService> _platformServiceFactory;
        public AccountPageViewModel(IIOService iOService, AuthService authService, GenericFactory<AccountType, IPlatformService> platformServiceFactory)
        {
            _iOService = iOService;
            _authService = authService;
            _platformServiceFactory = platformServiceFactory;
            _ = Initialize();
        }

        public async Task Initialize()
        {
            var accounts = _iOService.ReadData<List<AccountListItemViewModel>>(_authService.PasswordHash);
            AccountLists = new List<AccountListItemViewModel>(accounts);
            foreach (var account in accounts)
            {
                account.PlatformService = _platformServiceFactory.CreateImplementation(account.AccountType);
                account.Account.Id = (await account.PlatformService.TryFetchId(account.Account)).Item2;
                var rank = (await account.PlatformService.TryFetchRank(account.Account)).Item2;
                if (!string.IsNullOrEmpty(rank.Tier))
                    account.Rank = rank;
                account.Delete = () => RemoveAccount(account);
            }
            AccountLists = accounts;
            SaveList();

            if (!(UpdateView is null))
                UpdateView();
        }
        public async Task AddAccount(AccountListItemViewModel account)
        {
            AccountLists.Add(account);
            account.PlatformService = _platformServiceFactory.CreateImplementation(account.AccountType);
            account.Delete = () => RemoveAccount(account);
            account.Account.Id = (await account.PlatformService.TryFetchId(account.Account)).Item2;
            var rank = (await account.PlatformService.TryFetchRank(account.Account)).Item2;
            account.Rank = rank;
            SaveList();
            UpdateView();
        }
        public void RemoveAccount(AccountListItemViewModel account)
        {
            AccountLists.Remove(account);
            SaveList();
            UpdateView();
        }
        public void StartAddAccount()
        {
            NewAccount = new AccountListItemViewModel()
            {
                Account = new()
                {
                    Password = "",
                    Username = ""
                },
                Name = "",
            };
            AddAccountPromptShow = true;
        }
        public void CancelAddAccount()
        {
            AddAccountPromptShow = false;
        }
        public void FinishAddAccount()
        {
            _ = AddAccount(NewAccount);
            AddAccountPromptShow = false;
        }

        public void SaveList()
        {
            _iOService.UpdateData(AccountLists, _authService.PasswordHash);
        }
    }
}
