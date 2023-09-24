using Microsoft.AspNetCore.Components;
using AccountManager.Core.Models;
using AccountManager.Core.Interfaces;
using AccountManager.Infrastructure.Clients;

namespace AccountManager.Blazor.Components.AccountListTile.TileContent
{
    public partial class TileContent
    {
        [CascadingParameter]
        public Account? Account { get; set; }

        [Parameter]
        public EventCallback MouseEnterGraph { get; set; }

        [Parameter]
        public EventCallback MouseExitGraph { get; set; }
        public Rank? Rank { get; set; }

        private IPlatformService? _platformService;

        protected override void OnInitialized()
        {
            if (Account is null || _platformService is null)
                return;

            _platformService = _platformServiceFactory.CreateImplementation(Account.AccountType);
        }

        protected override async Task OnParametersSetAsync()
        {
            if (Account is null || _platformService is null)
                return;

            var rankResponse = await _platformService.TryFetchRank(Account);
            Rank = rankResponse.Item2;
            await InvokeAsync(() => StateHasChanged());
        }

        public void RefreshData()
        {
            if (Account is null)
                return;

            var cacheKeys = typeof(ILeagueGraphService).GetMembers()
            .Concat(typeof(IValorantGraphService).GetMembers())
            .Concat(typeof(ITeamFightTacticsGraphService).GetMembers())
            .Concat(typeof(ILeagueClient).GetMembers())
            .Concat(typeof(IValorantClient).GetMembers()).Select(method => $"{Account.Username}.{Account.AccountType}.{method.Name}");

            foreach (var key in cacheKeys)
            {
                _memoryCache.Remove(key);
                _distributedCache.Remove(key);
            }

            Account = new Account()
            {
                AccountType = Account.AccountType,
                Id = Account.Id,
                Name = Account.Name,
                PlatformId = Account.PlatformId,
                Password = Account.Password,
                Username = Account.Username,
            };

            InvokeAsync(() => StateHasChanged());
        }
    }
}