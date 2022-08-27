using Microsoft.AspNetCore.Components;
using AccountManager.Core.Models;
using AccountManager.Core.Models.RiotGames.Valorant.Responses;
using AccountManager.Core.Attributes;

namespace AccountManager.Blazor.Components.AccountListTile.TileContent.Pages.Valorant
{
    [AccountTilePage(Core.Enums.AccountType.Valorant, 0)]
    public partial class ValorantStorePage
    {
        public static int OrderNumber = 0;

        [Parameter]
        public Account Account { get; set; } = new();
        private Account _account = new();

        List<ValorantSkinLevelResponse>? storeFrontSkins;

        protected override void OnParametersSet()
        {
            if (_account != Account)
            {
                _account = Account;

                Task.Run(async () =>
                {
                    try
                    {
                        storeFrontSkins = await _valorantClient.GetValorantShopDeals(Account);
                    }
                    catch
                    {
                        storeFrontSkins = new();
                    }

                    await InvokeAsync(() => StateHasChanged());
                });
            }
        }
    }
}