using Microsoft.AspNetCore.Components;
using AccountManager.Core.Models;
using System.Reflection;
using AccountManager.Core.Models.UserSettings;

namespace AccountManager.Blazor.Components.AccountListTile
{
    public partial class ButtonContainer
    {
        [CascadingParameter, EditorRequired]
        public AccountListItemSettings Settings { get; set; } = new();
        [Parameter, EditorRequired]
        public Account Account { get; set; } = new();


        [Parameter, EditorRequired]
        public Action ReloadList { get; set; } = delegate { };

        [Parameter]
        public Action OpenEditModal { get; set; } = () => { };

        bool loginDisabled = false;
        string loginBtnStyle => loginDisabled ? "color:darkgrey; pointer-events: none;" : "";
        ConfirmationRequest? deleteAccountConfirmationRequest = null;
        async Task Login()
        {
            if (loginDisabled)
                return;
            loginDisabled = true;
            await _accountService.LoginAsync(Account);
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

        public void ExportAccount()
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
        public async Task ToggleContentViewAsync()
        {
            Settings.ShowAccountDetails = !Settings.ShowAccountDetails;
            await _accountItemSettings.SaveAsync();
        }
    }
}