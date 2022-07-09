using Microsoft.AspNetCore.Components;
using AccountManager.Core.Models;
using AccountManager.Core.Interfaces;
using AccountManager.Infrastructure.Clients;

namespace AccountManager.Blazor.Components.AccountListTile.TileContent
{
    public partial class TileContent
    {
        [Parameter]
        public Account Account { get; set; } = new();

        [Parameter]
        public EventCallback MouseEnterGraph { get; set; }

        [Parameter]
        public EventCallback MouseExitGraph { get; set; }

        public void RefreshData()
        {
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
                Guid = Account.Guid,
                Id = Account.Id,
                PlatformId = Account.PlatformId,
                Password = Account.Password,
                Username = Account.Username,
                Rank = Account.Rank,
            };

            InvokeAsync(() => StateHasChanged());
        }
    }
}