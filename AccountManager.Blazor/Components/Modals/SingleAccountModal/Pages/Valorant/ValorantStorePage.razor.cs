using Microsoft.AspNetCore.Components;
using AccountManager.Core.Models;
using AccountManager.Core.Models.RiotGames.Valorant.Responses;

namespace AccountManager.Blazor.Components.Modals.SingleAccountModal.Pages.Valorant
{
    public partial class ValorantStorePage
    {
        [Parameter, EditorRequired]
        public Account? Account { get; set; }

        [Parameter, EditorRequired]
        public Action? IncrementPage { get; set; }

        [Parameter, EditorRequired]
        public Action? DecrementPage { get; set; }

        public static string Title = "Store Front";
        List<ValorantSkinLevelResponse>? storeFrontSkins;
        protected override async Task OnInitializedAsync()
        {
            if (Account is null)
                return;

            storeFrontSkins = await _valorantClient.GetValorantShopDeals(Account);
        }
    }
}