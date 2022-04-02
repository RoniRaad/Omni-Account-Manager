using AccountManager.Core.Models;

namespace AccountManager.Blazor.Pages
{
    public partial class AccountList
    {
        private bool addAccountPrompt { get; set; } = false;
        private bool DragMode = false;
        protected override void OnInitialized()
        {
            _appState.Notify += () =>
            {
                InvokeAsync(() => StateHasChanged());
            };
        }
        public void SaveList()
        {
            _appState.SaveAccounts();
        }
        public void LoadList()
        {
            _ = Task.Run(async () =>
            {
                await _appState.UpdateAccounts();
            });
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

