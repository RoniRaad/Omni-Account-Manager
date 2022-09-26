using Microsoft.AspNetCore.Components;
using AccountManager.Core.Models;
using AccountManager.Core.Enums;
using AccountManager.Core.Models.UserSettings;

namespace AccountManager.Blazor.Components.AccountListTile
{
    public partial class AccountListTile
    {
        private AccountListItemSettings _settings { get; set; } = new AccountListItemSettings();
        [Parameter, EditorRequired]
        public Account Account { get; set; } = new ();
        [Parameter]
        public bool RenderButtons { get; set; }
        [Parameter, EditorRequired]
        public Action ReloadList { get; set; } = delegate { };
        [Parameter, EditorRequired]
        public Action OpenEditModal { get; set; } = delegate { };

        private bool graphIsHovered = false;
        private bool cardIsHovered = false;
        private bool dragSymbolIsHovered = false;
        private string cardStyle
        {
            get
            {
                if (graphIsHovered || dragSymbolIsHovered || !cardIsHovered || Account.AccountType == AccountType.TeamFightTactics) 
                    return "";
                return "box-shadow: 0px 0px 6px #424040; cursor: pointer;";
            }
        }

        private void OpenSingleAccountModal()
        {
            if (cardStyle == "")
                return;
            showFullTile = true;
        }

        private bool showFullTile = false;

        protected override void OnParametersSet()
        {
            if (!_accountItemSettings.Settings.TryGetValue(Account.Guid, out var settings))
            {
                settings = new() { AccountGuid = Account.Guid };
            }
            _accountItemSettings.Settings[Account.Guid] = settings;
            _settings = settings;

            base.OnParametersSet();
        }
    }
}