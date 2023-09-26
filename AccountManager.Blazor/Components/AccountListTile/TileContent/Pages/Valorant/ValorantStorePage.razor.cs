using Microsoft.AspNetCore.Components;
using AccountManager.Core.Models;
using AccountManager.Core.Models.RiotGames.Valorant.Responses;
using AccountManager.Core.Attributes;

namespace AccountManager.Blazor.Components.AccountListTile.TileContent.Pages.Valorant
{
    [AccountTilePage(Core.Enums.AccountType.Valorant, 0)]
    public partial class ValorantStorePage
    {
        [CascadingParameter]
        public Account? Account { get; set; }
        private Account _account = new();
        List<ValorantSkinLevelResponse>? storeFrontSkins;
        [CascadingParameter(Name = "RegisterTileDataRefresh")]
        Action<Action> RegisterTileDataRefresh { get; set; } = delegate { };
        protected override async Task OnInitializedAsync()
        {
            RegisterTileDataRefresh(() => Task.Run(GetValorantShopDeals));
            await base.OnInitializedAsync();
        }

        protected override async Task OnParametersSetAsync()
        {
            if (_account != Account && Account is not null)
            {
                _account = Account;

                await GetValorantShopDeals();
            }
        }
        private async Task GetValorantShopDeals()
        {
            if (Account is null)
                return;

            try
            {
                storeFrontSkins = await _valorantClient.GetValorantShopDeals(Account);
            }
            catch
            {
                storeFrontSkins = new();
            }

            await InvokeAsync(() => StateHasChanged());
        }
    }
}