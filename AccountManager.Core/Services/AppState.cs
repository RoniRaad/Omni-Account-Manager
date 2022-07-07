using AccountManager.Core.Interfaces;
using AccountManager.Core.Models;
using AccountManager.Core.Static;
using System.Collections.ObjectModel;

namespace AccountManager.Core.Services
{
    public class AppState
    {
        private readonly IAccountService _accountService;
        public RangeObservableCollection<Account> Accounts { get; set; }
        public bool IsInitialized { get; set; } = false;
        public AppState(IAccountService accountService)
        {
            _accountService = accountService;
            Accounts = new RangeObservableCollection<Account>();
            Accounts.AddRange(_accountService.GetAllAccountsMin());
          
            _ = UpdateAccounts();

            StartUpdateTimer();
        }

        public void StartUpdateTimer()
        {
            var timer = new System.Timers.Timer(TimeSpan.FromHours(1).TotalMilliseconds);
            timer.Elapsed += (sender, args) =>
            {
                _ = UpdateAccounts();
            };
            timer.AutoReset = true;
            timer.Enabled = true;
        }

        public async Task UpdateAccounts()
        {
            var minAccounts = _accountService.GetAllAccountsMin();

            Accounts.RemoveAll(acc => 
                !minAccounts.Any(minAcc => minAcc.Guid == acc.Guid));

            minAccounts.RemoveAll(
                acc => Accounts.Any(minAcc => minAcc.Guid == acc.Guid));

            Accounts.AddRange(minAccounts);

            var fullAccounts = new ObservableCollection<Account>(await _accountService.GetAllAccounts());
            Accounts.Clear();
            Accounts.AddRange(fullAccounts);
            IsInitialized = true;
        }

        public void SaveAccounts()
        {
            _accountService.WriteAllAccounts(Accounts.ToList());
        }

    }
}
