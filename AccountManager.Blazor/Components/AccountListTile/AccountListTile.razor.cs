using Microsoft.AspNetCore.Components;
using AccountManager.Core.Models;
using AccountManager.Core.Enums;
using AccountManager.Core.Models.UserSettings;
using System.Diagnostics;

namespace AccountManager.Blazor.Components.AccountListTile
{
    public partial class AccountListTile
    {
        private AccountListItemSettings _settings { get; set; } = new AccountListItemSettings();
        [CascadingParameter, EditorRequired]
        public Account? Account { get; set; }
        [Parameter]
        public bool RenderButtons { get; set; }
        [Parameter, EditorRequired]
        public Action ReloadList { get; set; } = delegate { };
        [Parameter, EditorRequired]
        public EventCallback<Account> OpenEditModal { get; set; }

        private bool graphIsHovered = false;
        private bool cardIsHovered = false;
        private bool dragSymbolIsHovered = false;
        private bool showFullTile = false;
        private bool shouldRender = false;
        private string cardStyle
        {
            get
            {
                if (graphIsHovered || dragSymbolIsHovered || !cardIsHovered || Account?.AccountType == AccountType.TeamFightTactics) 
                    return "";

                return "box-shadow: 0px 0px 6px #424040; cursor: pointer;";
            }
        }

        private void OpenSingleAccountModal()
        {
            if (cardStyle == "")
                return;

            shouldRender = true;
            showFullTile = true;
            StateHasChanged();
            shouldRender = false;
        }

        private void CloseSingleAccountModal()
        {
            showFullTile = false;
            
            shouldRender = true;
            InvokeAsync(() => StateHasChanged());
            shouldRender = false;
        }

        private void CardHovered()
        {
            if (cardIsHovered) return;
            
            cardIsHovered = true;
            shouldRender = true;
            StateHasChanged();
            shouldRender = false;

        }
        private void CardUnHovered()
        {
            if (!cardIsHovered) return;
            
            cardIsHovered = false;
            shouldRender = true;
            StateHasChanged();
            shouldRender = false;
        }

        protected override bool ShouldRender()
        {
            return shouldRender;
        }
        protected override void OnParametersSet()
        {
            if (Account is null)
                return;

            if (!_accountItemSettings.Settings.TryGetValue(Account.Id, out var settings))
            {
                settings = new() { AccountGuid = Account.Id };
            }

            _accountItemSettings.Settings[Account.Id] = settings;
            _settings = settings;

            base.OnParametersSet();
        }
    }
}