using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using System.Net.Http;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Routing;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.Web.Virtualization;
using Microsoft.JSInterop;
using AccountManager.Blazor.Shared;
using AccountManager.Blazor;
using Plk.Blazor.DragDrop;
using AccountManager.Core.Interfaces;
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
        List<ValorantSkinLevelResponse> storeFrontSkins = new();
        private bool noDataReturned = false;
        protected override async Task OnInitializedAsync()
        {
            if (Account is null)
                return;
            storeFrontSkins.Clear();
            var items = await _riotClient.GetValorantShopDeals(Account);
            foreach (var item in items)
            {
                storeFrontSkins.Add(item);
            }

            if (!items.Any())
                noDataReturned = true;
        }
    }
}