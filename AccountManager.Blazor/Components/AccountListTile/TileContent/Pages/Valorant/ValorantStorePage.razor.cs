using Microsoft.AspNetCore.Components;
using AccountManager.Core.Models;
using AccountManager.Core.Models.RiotGames.Valorant.Responses;
using AccountManager.Core.Attributes;
using AccountManager.Blazor.State;

namespace AccountManager.Blazor.Components.AccountListTile.TileContent.Pages.Valorant
{
    [AccountTilePage(Core.Enums.AccountType.Valorant, 0)]
    public partial class ValorantStorePage
    {
        [Parameter]
        public Account Account { get; set; } = new();
        private Account _account = new();
        [CascadingParameter]
        public IAccountListItem? AccountListItem { get; set; }
        private ValorantAccountListItem? _accountListItem;

        protected override void OnParametersSet()
        {
            if (AccountListItem is not null)
                _accountListItem = AccountListItem as ValorantAccountListItem;

            if (_account != Account)
            {
                _account = Account;

                if (_accountListItem is not null)
                    Task.Run(async () =>
                    {
                        await _accountListItem.RefreshData();
                        await InvokeAsync(() => StateHasChanged());
                    });
            }
        }
    }
}