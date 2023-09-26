using AccountManager.Core.Models;
using Microsoft.JSInterop;

namespace AccountManager.Blazor.Pages
{
    public partial class AccountList
    {
        private Account? editAccountTarget;
        private bool addAccountPrompt { get; set; } = false;
        private int amountOfAccountsFilered;

        protected override async Task OnInitializedAsync()
        {
            await _appState.UpdateAccounts();
            _accountFilterService.OnFilterChanged += () => LoadList();
            amountOfAccountsFilered = _appState?.Accounts?.Count(acc => !_accountFilterService.AccountTypeFilter.Contains(acc.AccountType) || acc?.Name?.ToLower()?.Contains(_accountFilterService.AccountNameFilter.ToLower()) is false) ?? 0;
			await _jsRuntime.InvokeVoidAsync("startObserver");
        }

        protected async override Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
				amountOfAccountsFilered = _appState?.Accounts?.Count(acc => !_accountFilterService.AccountTypeFilter.Contains(acc.AccountType) || acc?.Name?.ToLower()?.Contains(_accountFilterService.AccountNameFilter.ToLower()) is false) ?? 0;
			}
		}

        public void LoadList()
        {
            amountOfAccountsFilered = _appState?.Accounts?.Count(acc => !_accountFilterService.AccountTypeFilter.Contains(acc.AccountType) || acc?.Name?.ToLower()?.Contains(_accountFilterService.AccountNameFilter.ToLower()) is false) ?? 0;
            InvokeAsync(StateHasChanged);
        }

        public void StartAddAccount()
        {
            addAccountPrompt = true;
        }

        public void CancelAddAccount()
        {
            addAccountPrompt = false;
        }

        public void FinishAddAccount()
        {
            addAccountPrompt = false;
        }

        public void OnDrop() => _appState.SaveAccountOrder();

        public string AccountListItemClass(Account account)
        {
            return _accountFilterService.AccountTypeFilter.Contains(account.AccountType) && account?.Name?.ToLower()?.Contains(_accountFilterService.AccountNameFilter.ToLower()) is true ? "col-md-4 col-sm-6 col-xxl-2 col-xxxl-2" : "d-none";
        }

        public void OpenEditModal(Account account)
        {
            editAccountTarget = account;
            InvokeAsync(() => StateHasChanged());
        }
    }
}

