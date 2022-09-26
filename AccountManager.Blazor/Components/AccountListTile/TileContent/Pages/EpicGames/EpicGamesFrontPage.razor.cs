using Microsoft.AspNetCore.Components;
using AccountManager.Core.Models;
using Microsoft.Extensions.Caching.Distributed;
using AccountManager.Core.Static;
using AccountManager.Core.Attributes;
using NuGet;
using AccountManager.Core.Models.EpicGames;

namespace AccountManager.Blazor.Components.AccountListTile.TileContent.Pages.EpicGames
{
    [AccountTilePage(Core.Enums.AccountType.EpicGames, 0)]
    public partial class EpicGamesFrontPage
    {
        private Account _account = new();
        private bool epicInstallNotFound = false;
        [Parameter]
        public Account Account { get; set; } = new();
        List<EpicGamesInstalledGame> Games { get; set; } = new();

        protected override void OnInitialized()
        {
            _account = Account;
        }

        protected override async Task OnParametersSetAsync()
        {
             _account = Account;

            await base.OnParametersSetAsync();
        }
        private string selectedEpicGame = "none";

        public void SetGame(string appId)
        {
            _persistantCache.SetString($"{Account.Guid}.SelectedEpicGame", appId);
        }

        public void OnRadioClicked(ChangeEventArgs args)
        {
            SetGame(args?.Value?.ToString() ?? "none");
            selectedEpicGame = args?.Value?.ToString() ?? "none";
        }
        public async Task RefreshGamesAsync()
        {
            Games.Clear();

            selectedEpicGame = await _persistantCache.GetStringAsync($"{Account.Guid}.SelectedEpicGame") ?? "none";

            if (!_steamLibraryService.TryGetInstalledGames(out var gameManifests))
                return;

            Games.AddRange(gameManifests);
        }

        protected async override Task OnAfterRenderAsync(bool firstRender)
        {
            var cachedSelectedGame = await _persistantCache.GetStringAsync($"{Account.Guid}.SelectedEpicGame") ?? "none";
            if (cachedSelectedGame != selectedEpicGame)
            {
                selectedEpicGame = cachedSelectedGame;
                StateHasChanged();
            }
            await base.OnAfterRenderAsync(firstRender);
        }

        protected async override Task OnInitializedAsync()
        {
            await RefreshGamesAsync();
            await base.OnInitializedAsync();
        }
    }
}