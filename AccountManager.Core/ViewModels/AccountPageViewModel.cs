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
        private GenericFactory<AccountType, IPlatformService> _platformServiceFactory;
        public AccountPageViewModel(IIOService iOService, AuthService authService, GenericFactory<AccountType, IPlatformService> platformServiceFactory, IRiotClient riotClient, IRankingService rankingService)
        {
            _iOService = iOService;
            _authService = authService;
            _platformServiceFactory = platformServiceFactory;
            _riotClient = riotClient;
            _rankingService = rankingService;
        }

        public async Task InitializeView()
        {
            var accounts = _iOService.ReadData<List<AccountListItemViewModel>>(_authService.PasswordHash);
            AccountLists = new List<AccountListItemViewModel>();
            foreach (var account in accounts)
            {
                account.PlatformService = _platformServiceFactory.CreateImplementation(account.AccountType);
                account.Account.Id = await account.PlatformService.TryFetchId(account.Account);
                account.Rank = await account.PlatformService.TryFetchRank(account.Account);
                account.Delete = () => RemoveAccount(account);
            }
            _iOService.UpdateData(accounts, _authService.PasswordHash);
            AccountLists = accounts;
        }
        public async Task AddAccount(AccountListItemViewModel account)
        {
            account.PlatformService = _platformServiceFactory.CreateImplementation(account.AccountType);
            account.Delete = () => RemoveAccount(account);
            account.Account.Id = await account.PlatformService.TryFetchId(account.Account);
            account.Rank = await account.PlatformService.TryFetchRank(account.Account);
            
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
