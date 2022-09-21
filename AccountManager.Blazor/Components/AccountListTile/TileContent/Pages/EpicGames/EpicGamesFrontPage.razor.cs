using Microsoft.AspNetCore.Components;
using AccountManager.Core.Models;
using Blazorise.Charts;
using AccountManager.Core.Models.Steam;
using Microsoft.Extensions.Caching.Distributed;
using AccountManager.Core.Static;
using AccountManager.Core.Attributes;
using NuGet;
using static AccountManager.Infrastructure.Services.EpicGamesLibraryService;

namespace AccountManager.Blazor.Components.AccountListTile.TileContent.Pages.EpicGames
{
    [AccountTilePage(Core.Enums.AccountType.EpicGames, 0)]
    public partial class EpicGamesFrontPage
    {
        public static int OrderNumber = 0;
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
            if (_account != Account)
            {
                _account = Account;
            }

            await base.OnParametersSetAsync();
        }
        public string SelectedEpicGame = "none";

        public void SetGame(string appId)
        {
            _persistantCache.SetString($"{Account.Guid}.SelectedEpicGame", appId);
        }

        public void OnRadioClicked(ChangeEventArgs args)
        {
            SetGame(args?.Value?.ToString() ?? "none");
            SelectedEpicGame = args?.Value?.ToString() ?? "none";
        }
        public async Task RefreshGamesAsync()
        {
            Games.Clear();

            SelectedEpicGame = await _persistantCache.GetStringAsync($"{Account.Guid}.SelectedEpicGame") ?? "none";

            if (!_steamLibraryService.TryGetInstalledGames(out var gameManifests))
                return;

            Games.AddRange(gameManifests);
        }

        protected async override Task OnAfterRenderAsync(bool first)
        {
            var cachedSelectedGame = await _persistantCache.GetStringAsync($"{Account.Guid}.SelectedEpicGame") ?? "none";
            if (cachedSelectedGame != SelectedEpicGame)
            {
                SelectedEpicGame = cachedSelectedGame;
                StateHasChanged();
            }
            await base.OnAfterRenderAsync(first);
        }

        protected async override Task OnInitializedAsync()
        {
            await RefreshGamesAsync();
            await base.OnInitializedAsync();
        }
    }
}