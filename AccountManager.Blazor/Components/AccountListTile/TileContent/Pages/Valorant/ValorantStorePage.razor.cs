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

        List<ValorantSkinLevelResponse> storeFrontSkins = new();
        protected override async Task OnInitializedAsync()
        {
            if (Account is null)
                return;

            _account = Account;

            storeFrontSkins.Clear();
            var items = await _valorantClient.GetValorantShopDeals(Account);

            foreach (var item in items)
            {
                storeFrontSkins.Add(item);
            }
        }

        protected override void OnInitialized()
        {
        }

        protected override async Task OnParametersSetAsync()
        {
            if (_account != Account)
            {
                _account = Account;

                storeFrontSkins.Clear();
                var items = await _valorantClient.GetValorantShopDeals(Account);

                foreach (var item in items)
                {
                    storeFrontSkins.Add(item);
                }
            }
        }
    }
}