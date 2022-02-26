using AccountManager.Core.Enums;
using AccountManager.Core.Factories;
using AccountManager.Core.Interfaces;
using AccountManager.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

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
        private GenericFactory<AccountType, ILoginService> _loginServiceFactory;
        public AccountPageViewModel(IIOService iOService, AuthService authService, GenericFactory<AccountType, ILoginService> loginServiceFactory)
        {
            _iOService = iOService;
            _authService = authService;
            _loginServiceFactory = loginServiceFactory;
            var accounts = _iOService.ReadData<List<AccountListItemViewModel>>(_authService.PasswordHash);
            AccountLists = new List<AccountListItemViewModel>();
            foreach (var account in accounts)
            {
                account.LoginService = _loginServiceFactory.CreateImplementation(account.AccountType);
                account.Delete = () => RemoveAccount(account);
            }
            AccountLists = accounts;
        }

        public void AddAccount(AccountListItemViewModel account)
        {
            account.LoginService = _loginServiceFactory.CreateImplementation(account.AccountType);
            account.Delete = () => RemoveAccount(account);
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
