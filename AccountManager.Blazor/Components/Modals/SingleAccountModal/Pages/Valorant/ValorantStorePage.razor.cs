using Microsoft.AspNetCore.Components;
using AccountManager.Core.Models;
using AccountManager.Core.Models.RiotGames.Valorant.Responses;
using AccountManager.Core.Attributes;

namespace AccountManager.Blazor.Components.Modals.SingleAccountModal.Pages.Valorant
{
    [SingleAccountPage("Store Front", Core.Enums.AccountType.Valorant, 0)]
    public partial class ValorantStorePage
    {
        [Parameter, EditorRequired]
        public Account? Account { get; set; }

        [Parameter, EditorRequired]
        public Action? IncrementPage { get; set; }

        [Parameter, EditorRequired]
        public Action? DecrementPage { get; set; }

        List<ValorantSkinLevelResponse>? storeFrontSkins;
        protected override async Task OnInitializedAsync()
        {
            if (Account is null)
                return;

            try
            {
                storeFrontSkins = await _valorantClient.GetValorantShopDeals(Account);
            }
            catch
            {
                _alertService.AddErrorAlert("Unable to get store information for valorant account.");
            }
        }
    }
}