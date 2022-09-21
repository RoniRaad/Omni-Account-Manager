using AccountManager.Core.Interfaces;
using AccountManager.Core.Models;
using Microsoft.AspNetCore.Components;

namespace AccountManager.Blazor.Components
{
    public partial class AccountEditModal
    {

        [Parameter, EditorRequired]
        public Account Account { get; set; } = new ();
        [Parameter, EditorRequired]
        public Action Close { get; set; } = delegate { };
        private bool passwordVisible = false;
        public string PasswordType
        {
            get { return passwordVisible ? "" : "password"; }
        }
        public string PasswordToggleButtonColor
        {
            get { return passwordVisible ? "var(--primary-dark)" : "var(--secondary-dark)"; }
        }
        public void Submit()
        {
            var account = _appState.Accounts.FirstOrDefault((acc) => acc.Guid == Account.Guid);
            if (account is null)
                return;

            account.Password = Account.Password;
            account.Id = Account.Id;

            _appState.SaveAccounts();
            Close();
        }
    }
}
