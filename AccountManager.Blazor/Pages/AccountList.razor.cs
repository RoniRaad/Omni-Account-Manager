using AccountManager.Core.Models;

namespace AccountManager.Blazor.Pages
{
    public partial class AccountList
    {
        private bool addAccountPrompt { get; set; } = false;
        public List<Account> ListItems = new List<Account>();
        private bool DragMode = false;
        protected override async Task OnInitializedAsync()
        {
            LoadList();
            await base.OnInitializedAsync();
        }
        public void SaveList()
        {
            _accountService.WriteAllAccounts(ListItems);
        }
        public void LoadList()
        {
            var accounts = _accountService.GetAllAccounts();
            ListItems = accounts;
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

