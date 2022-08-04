using AccountManager.Core.Models;
using Microsoft.JSInterop;

namespace AccountManager.Blazor.Pages
{
    public partial class AccountList
    {
        private Account? editAccountTarget;
        private bool addAccountPrompt { get; set; } = false;

        protected override void OnInitialized()
        {
            _appState.UpdateAccounts();
            _accountFilterService.OnFilterChanged += () => LoadList();
        }

        protected override void OnAfterRender(bool first)
        {
            if (first)
                Task.Run(async () => await _jsRuntime.InvokeVoidAsync("appendElement", "accounts-grid", "new-account-placeholder"));
        }

        public void SaveList()
        {
            _appState.SaveAccounts();
        }

        public void LoadList()
        {
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

