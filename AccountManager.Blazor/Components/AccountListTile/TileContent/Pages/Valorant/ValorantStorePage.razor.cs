using Microsoft.AspNetCore.Components;
using AccountManager.Core.Models;
using AccountManager.Core.Models.RiotGames.Valorant.Responses;
using System.Security.Principal;


namespace AccountManager.Blazor.Components.AccountListTile.TileContent.Pages.Valorant
{
    public partial class ValorantStorePage
    {
        public static int OrderNumber = 0;

        [Parameter]
        public Account Account { get; set; } = new();
        private Account _account = new();

        List<ValorantSkinLevelResponse>? storeFrontSkins;
        protected override void OnInitialized()
        {
            if (Account is null)
                return;

            _account = Account;

            Task.Run(async () =>
            {
                storeFrontSkins = await _valorantClient.GetValorantShopDeals(Account);
                await InvokeAsync(() => StateHasChanged());
            });
        }

        protected override void OnParametersSet()
        {
            if (_account != Account)
            {
                _account = Account;

                Task.Run(async () =>
                {
                    storeFrontSkins = await _valorantClient.GetValorantShopDeals(Account);
                    await InvokeAsync(() => StateHasChanged());
                });
            }
        }
    }
}