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
            _appState.Accounts.Add(NewAccount);
            _appState.SaveAccounts();
            Close();
        }
    }
}
