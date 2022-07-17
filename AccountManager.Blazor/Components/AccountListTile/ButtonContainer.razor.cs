using Microsoft.AspNetCore.Components;
using AccountManager.Core.Interfaces;
using AccountManager.Core.Models;

namespace AccountManager.Blazor.Components.AccountListTile
{
    public partial class ButtonContainer
    {
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
        bool showExportModal = false;
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
                        AccountService.RemoveAccount(Account);
                        ReloadList();
                    }

                    deleteAccountConfirmationRequest = null;
                    InvokeAsync(() => StateHasChanged());
                }, 
                RequestMessage = "Are you sure you want to delete this account? This can NOT be undone."
            };
        }

        public void Export()
        {
            showExportModal = true;
        }
    }
}