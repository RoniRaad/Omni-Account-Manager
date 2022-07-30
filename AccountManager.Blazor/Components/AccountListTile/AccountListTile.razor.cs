using Microsoft.AspNetCore.Components;
using AccountManager.Core.Models;
using AccountManager.Core.Interfaces;
using AccountManager.Core.Enums;
using AccountManager.Core.Models.UserSettings;

namespace AccountManager.Blazor.Components.AccountListTile
{
    public partial class AccountListTile
    {
        private AccountListItemSettings _settings { get; set; } = new AccountListItemSettings();
        [Parameter, EditorRequired]
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public Account Account { get; set; }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        [Parameter]
        public bool RenderButtons { get; set; }
        [Parameter, EditorRequired]
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public Action ReloadList { get; set; }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        [Parameter, EditorRequired]
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public Action OpenEditModal { get; set; }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

        [Parameter, EditorRequired]
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public IAccountService AccountService { get; set; }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        private bool graphIsHovered = false;
        private bool cardIsHovered = false;
        private bool dragSymbolIsHovered = false;
        bool loginDisabled = false;
        string loginBtnStyle => loginDisabled ? "color:darkgrey; pointer-events: none;" : "";
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

        protected override void OnInitialized()
        {
            if (!_accountItemSettings.Settings.TryGetValue(Account.Guid, out var settings))
            {
                settings = new() { AccountGuid = Account.Guid };
            }
            _accountItemSettings.Settings[Account.Guid] = settings;
            _settings = settings;

            base.OnInitialized();
        }
    }
}