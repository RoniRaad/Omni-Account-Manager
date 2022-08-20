using Microsoft.AspNetCore.Components;
using AccountManager.Core.Interfaces;
using AccountManager.Core.Models;
using System.Reflection;
using AccountManager.Core.Models.UserSettings;
using AccountManager.Blazor.Pages;

namespace AccountManager.Blazor.Components.AccountListTile
{
    public partial class ButtonContainer
    {
        [CascadingParameter, EditorRequired]
        public AccountListItemSettings Settings { get; set; } = new();
        [Parameter, EditorRequired]
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

        public Account Account { get; set; }

#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

        [Parameter, EditorRequired]
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

        public Action ReloadList { get; set; }

#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

        [Parameter]
        public Action OpenEditModal { get; set; } = () => { };

        [Parameter, EditorRequired]
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

        public IAccountService AccountService { get; set; }

#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

        bool loginDisabled = false;
        string loginBtnStyle => loginDisabled ? "color:darkgrey; pointer-events: none;" : "";
        ConfirmationRequest? deleteAccountConfirmationRequest = null;
        async Task Login()
        {
            if (loginDisabled)
                return;
            loginDisabled = true;
            await AccountService.Login(Account);
            loginDisabled = false;
        }

        public void Delete()
        {
            deleteAccountConfirmationRequest = new ConfirmationRequest()
            {
                Callback = (userConfirmed) =>
                {
                    if (userConfirmed)
                    {
                        _appState.Accounts.RemoveAll((acc) => acc.Guid == Account.Guid);
                        _appState.SaveAccounts();
                        ReloadList();
                    }

                    deleteAccountConfirmationRequest = null;
                    InvokeAsync(() => StateHasChanged());
                }, 
                RequestMessage = "Are you sure you want to delete this account? This can NOT be undone."
            };
        }

        public void CreateShortcut()
        {
            if (Account?.Id is null)
                return;

            var platformService = _platformServiceFactory.CreateImplementation(Account.AccountType);
            var icoPath = platformService.GetType()?.GetField("IcoFilePath", BindingFlags.Static | BindingFlags.FlattenHierarchy | BindingFlags.Public)?.GetValue(null)?.ToString() ?? "";
            var successful = _shortcutService.TryCreateDesktopLoginShortcut(Account.Id, Account.Guid, icoPath);
             
            if (successful)
                _alertService.AddInfoAlert("Shortcut created successfully!");
            else
                _alertService.AddErrorAlert("There was an error creating the desktop shortcut!");
        }

        public void ToggleContentView()
        {
            Settings.ShowAccountDetails = !Settings.ShowAccountDetails;
            _accountItemSettings.Save();
        }
    }
}