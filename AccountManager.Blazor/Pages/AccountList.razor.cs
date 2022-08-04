using AccountManager.Core.Models;

namespace AccountManager.Blazor.Pages
{
    public partial class AccountList
    {
        private Account? editAccountTarget;
        private bool addAccountPrompt { get; set; } = false;

        protected override void OnInitialized()
        {
            _accountFilterService.OnFilterChanged += () => InvokeAsync(() => StateHasChanged());
            _accountService.OnAccountListChanged += () => InvokeAsync(() => StateHasChanged());
        }

        public void SaveList()
        {
            _appState.SaveAccounts();
        }

        public void LoadList()
        {
            _ = _appState.UpdateAccounts();
            InvokeAsync(() => StateHasChanged());
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
    }
}

