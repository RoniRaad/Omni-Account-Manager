using AccountManager.Core.Enums;
using AccountManager.Core.Factories;
using AccountManager.Core.Interfaces;
using AccountManager.Core.Models;
using AccountManager.Core.Services;
using AccountManager.Infrastructure.Clients;
using AccountManager.Infrastructure.Services.RankingServices;
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
        private IRiotClient _riotClient;
        private IRankingService _rankingService;
        private AuthService _authService;
        private GenericFactory<AccountType, ILoginService> _loginServiceFactory;
        public AccountPageViewModel(IIOService iOService, AuthService authService, GenericFactory<AccountType, ILoginService> loginServiceFactory, IRiotClient riotClient, IRankingService rankingService)
        {
            _iOService = iOService;
            _authService = authService;
            _loginServiceFactory = loginServiceFactory;
            _riotClient = riotClient;
            _rankingService = rankingService;
        }

        public async Task InitializeView()
        {
            var accounts = _iOService.ReadData<List<AccountListItemViewModel>>(_authService.PasswordHash);
            AccountLists = new List<AccountListItemViewModel>();
            foreach (var account in accounts)
            {
                if (account.AccountType != AccountType.Steam)
                {
                    if (string.IsNullOrEmpty(account.Account.Id))
                        account.Account.Id = await _riotClient.GetPuuId(account.Account.Username, account.Account.Password);
                }
                
                account.LoginService = _loginServiceFactory.CreateImplementation(account.AccountType);
                account.Delete = () => RemoveAccount(account);
}
            _iOService.UpdateData(accounts, _authService.PasswordHash);

            AccountLists = accounts;
            _ = Task.Factory.StartNew(async () =>
            {
                AccountLists = await _rankingService.TryFetchRanks(accounts);
                UpdateView();
            });
        }

        public async Task AddAccount(AccountListItemViewModel account)
        {
            account.LoginService = _loginServiceFactory.CreateImplementation(account.AccountType);
            account.Delete = () => RemoveAccount(account);
            if (account.AccountType != AccountType.Steam)
                account.Account.Id = await _riotClient.GetPuuId(account.Account.Username, account.Account.Password);
            AccountLists.Add(account);
        }

        public void RemoveAccount(AccountListItemViewModel account)
        {
            AccountLists.Remove(account);
            _iOService.UpdateData(AccountLists, _authService.PasswordHash);
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
            AddAccount(NewAccount);
            _iOService.UpdateData(AccountLists, _authService.PasswordHash);
            AddAccountPromptShow = false;
        }
    }
}
