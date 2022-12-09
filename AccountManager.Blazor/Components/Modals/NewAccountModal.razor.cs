using AccountManager.Core.Interfaces;
using AccountManager.Core.Models;
using Microsoft.AspNetCore.Components;

namespace AccountManager.Blazor.Components.Modals
{
    public partial class NewAccountModal
    {
        public Account NewAccount { get; set; } = new();
        [Parameter, EditorRequired]
        public Action Close { get; set; } = delegate { };
    
        public void AddAccount()
        {
            if (string.IsNullOrEmpty(NewAccount.Name) || string.IsNullOrEmpty(NewAccount.Username) || string.IsNullOrEmpty(NewAccount.Password))
                return;

            _appState.Accounts.Add(NewAccount);
            _accountService.SaveAccountAsync(NewAccount);
            Close();
        }
    }
}
