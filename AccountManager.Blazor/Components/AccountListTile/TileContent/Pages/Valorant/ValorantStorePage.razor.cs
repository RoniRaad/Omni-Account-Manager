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
        protected override async Task OnInitializedAsync()
        {
            if (Account is null)
                return;

            _account = Account;

            storeFrontSkins = await _valorantClient.GetValorantShopDeals(Account);
        }

        protected override async Task OnParametersSetAsync()
        {
            if (_account != Account)
            {
                _account = Account;

                var storeFrontSkins = await _valorantClient.GetValorantShopDeals(Account);
            }
        }
    }
}