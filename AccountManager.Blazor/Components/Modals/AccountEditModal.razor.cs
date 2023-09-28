using AccountManager.Core.Interfaces;
using AccountManager.Core.Models;
using AccountManager.Infrastructure.Repositories;
using Microsoft.AspNetCore.Components;

namespace AccountManager.Blazor.Components
{
    public partial class AccountEditModal
    {

        [Parameter, EditorRequired]
        public Account? Account { get; set; }
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
        public async Task Submit()
        {
            if (Account is null)
                return;

            var account = _appState.Accounts.FirstOrDefault((acc) => acc.Id == Account.Id);
            if (account is null)
                return;

            account.Password = Account.Password;
            account.Name = Account.Name;

            await _accountService.SaveAccountAsync(account);
            Close();
        }
    }
}
