using AccountManager.Core.Interfaces;
using AccountManager.Core.Models;
using Microsoft.AspNetCore.Components;

namespace AccountManager.Blazor.Components
{
    public partial class AccountEditModal
    {

        [Parameter, EditorRequired]
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public Account Account { get; set; }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        [Parameter, EditorRequired]
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public IAccountService AccountService { get; set; }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        [Parameter, EditorRequired]
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public Action Close { get; set; }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
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
