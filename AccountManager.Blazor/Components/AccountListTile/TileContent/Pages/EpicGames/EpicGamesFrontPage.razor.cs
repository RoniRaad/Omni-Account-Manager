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
        private bool epicInstallNotFound = false;
        [CascadingParameter]
        public Account? Account { get; set; }
        [CascadingParameter(Name = "RegisterTileDataRefresh")]
        Action<Action> RegisterTileDataRefresh { get; set; } = delegate { };
        List<EpicGamesInstalledGame> Games { get; set; } = new();
        private string selectedEpicGame = "none";

        protected override async Task OnParametersSetAsync()
        {
            await base.OnParametersSetAsync();
        }
        private async Task UpdateGames()
        {
            if (Account is null)
                return;

            await RefreshGamesAsync();
        }

        public void SetGame(string appId)
        {
            if (Account is null)
                return;

            _persistantCache.SetString($"{Account.Id}.SelectedEpicGame", appId);
        }

        public void OnRadioClicked(ChangeEventArgs args)
        {
            SetGame(args?.Value?.ToString() ?? "none");
            selectedEpicGame = args?.Value?.ToString() ?? "none";
        }
        public async Task RefreshGamesAsync()
        {
            Games.Clear();

            selectedEpicGame = await _persistantCache.GetStringAsync($"{Account.Id}.SelectedEpicGame") ?? "none";

            if (!_steamLibraryService.TryGetInstalledGames(out var gameManifests))
                return;

            Games.AddRange(gameManifests);
        }

        protected async override Task OnAfterRenderAsync(bool firstRender)
        {
            var cachedSelectedGame = await _persistantCache.GetStringAsync($"{Account.Id}.SelectedEpicGame") ?? "none";
            if (cachedSelectedGame != selectedEpicGame)
            {
                selectedEpicGame = cachedSelectedGame;
                StateHasChanged();
            }
            await base.OnAfterRenderAsync(firstRender);
        }

        protected async override Task OnInitializedAsync()
        {
            RegisterTileDataRefresh(() => Task.Run(UpdateGames));
            await RefreshGamesAsync();
            await base.OnInitializedAsync();
        }
    }
}