using AccountManager.Core.Enums;
using AccountManager.Core.Models;
using AccountManager.Core.Services;
using AccountManager.Core.ViewModels;
using AccountManager.Infrastructure.Clients;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace AccountManager.Infrastructure.Services.RankingServices
{
    public class LeagueRankingService
    {
        public AccountType RelevantAccountType = AccountType.League;
        private IIOService _iOService;
        private AuthService _authService;
        private LeagueClient _leagueClient;
        public LeagueRankingService(IIOService iOService, AuthService authService, LeagueClient leagueClient)
        {
            _iOService = iOService;
            _authService = authService;
            _leagueClient = leagueClient;

            var aTimer = new System.Timers.Timer(5 * 60000);
            // Hook up the Elapsed event for the timer. 
            aTimer.Elapsed += OnTimedEvent;
            aTimer.AutoReset = true;
            aTimer.Enabled = true;
        }
        public Task OnTimedEvent
        public async Task UpdateCurrentPuuid()
        {
            var league = await _leagueClient.GetSignedInAccountAsync();
            if (league is null)
                return;

            if (!string.IsNullOrEmpty(league.Puuid) || !string.IsNullOrEmpty(league.Username))
            {

                var accounts = _iOService.ReadData<List<AccountListItemViewModel>>(_authService.PasswordHash);

                foreach (var account in accounts)
                {
                    if (account.AccountType != RelevantAccountType || league.Username != account.Account.Username)
                        continue;

                    account.Account.Id = league.Puuid;
                }

                _iOService.UpdateData(accounts, _authService.PasswordHash);
            }
        }

        public async Task TryFetchRanks()
        {
            await UpdateCurrentPuuid();

            var accounts = _iOService.ReadData<List<AccountListItemViewModel>>(_authService.PasswordHash);

            foreach (var account in accounts)
            {
                if (account.AccountType != RelevantAccountType || account.Account.Id is null)
                    continue;

                account.Rank = await _leagueClient.GetRankByPuuidAsync(account.Account.Id);
            }

            _iOService.UpdateData(accounts, _authService.PasswordHash);
        }
        public string GetCommandLineValue(string commandline, string key)
        {
            key += "=";
            var valueStart = commandline.IndexOf(key) + key.Length;
            var valueEnd = commandline.IndexOf(" ", valueStart);
            return commandline.Substring(valueStart, valueEnd - valueStart).Replace(@"\", "").Replace("\"", "");
        }
        private string GetRiotExePath()
        {
            return @"C:\Riot Games\Riot Client\RiotClientServices.exe";
        }
    }
}
