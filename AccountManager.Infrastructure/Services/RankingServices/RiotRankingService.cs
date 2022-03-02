using AccountManager.Core.Enums;
using AccountManager.Core.Models;
using AccountManager.Core.Services;
using AccountManager.Core.ViewModels;
using AccountManager.Infrastructure.Clients;
using System;
using System.Collections.Generic;
using System.Diagnostics;
namespace AccountManager.Infrastructure.Services.RankingServices
{
    public class RiotRankingService : IRankingService
    {
        public readonly AccountType RelevantAccountType = AccountType.League;
        private readonly IIOService _iOService;
        private readonly AuthService _authService;
        private readonly LeagueClient _leagueClient;
        private readonly IRiotClient _riotClient;
        public RiotRankingService(IIOService iOService, AuthService authService, LeagueClient leagueClient, IRiotClient riotClient)
        {
            _iOService = iOService;
            _authService = authService;
            _leagueClient = leagueClient;
            _riotClient = riotClient;
        }
        public async Task UpdateCurrentPuuid()
        {
            var accounts = _iOService.ReadData<List<AccountListItemViewModel>>(_authService.PasswordHash);

            foreach (var account in accounts)
            {
                if (account.AccountType == AccountType.Steam || !string.IsNullOrEmpty(account.Account.Id))
                    continue;

                var puuid = await _riotClient.GetPuuId(account.Account.Username, account.Account.Password);

                account.Account.Id = puuid;
            }

            _iOService.UpdateData(accounts, _authService.PasswordHash);

        }
        public async Task<List<AccountListItemViewModel>> TryFetchRanks(List<AccountListItemViewModel> accounts)
        {
            foreach (var account in accounts)
            {
                if (account.AccountType == AccountType.Steam || string.IsNullOrEmpty(account.Account.Id))
                    continue;

                if (account.AccountType == AccountType.League)
                {
                    if (string.IsNullOrEmpty(account.Rank))
                        account.Rank = await _leagueClient.GetRankByPuuidAsync(account.Account.Id);
                }
                else if (account.AccountType == AccountType.Valorant)
                    account.Rank = await _riotClient.GetValorantRank(account.Account);
            }

            return accounts;
        }
        public async Task<AccountListItemViewModel> TryFetchRank(AccountListItemViewModel account)
        {

            if (account.AccountType == AccountType.Steam || string.IsNullOrEmpty(account.Account.Id))
                return account;

            if (account.AccountType == AccountType.League)
            {
                if (string.IsNullOrEmpty(account.Rank))
                    account.Rank = await _leagueClient.GetRankByPuuidAsync(account.Account.Id);
            }
            else if (account.AccountType == AccountType.Valorant)
                account.Rank = await _riotClient.GetValorantRank(account.Account);

            return account;
        }
    }
}
