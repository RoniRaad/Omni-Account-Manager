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
        public event Action AccountsChanged;
        public event Action UpdateGraphs;
        public AppState(IAccountService accountService)
        {
            _accountService = accountService;
            Accounts = new RangeObservableCollection<Account>();
            Accounts.AddRange(_accountService.GetAllAccountsMin());
            UpdateGraphs = delegate { };
            AccountsChanged = delegate
            {
                UpdateGraphs.Invoke();
            };
            Accounts.CollectionChanged += async (s, e) => {
                Accounts?.RemoveAll((account) => Accounts.Count((innerAccount) => account.Guid == innerAccount.Guid) > 1);

                if (Accounts?.ToList() is not null)
                    _accountService.WriteAllAccounts(Accounts?.ToList() ?? new());

                await Task.Delay(1); // Fixes UI bugs with elements dependent on the Accounts property
                AccountsChanged.Invoke();
            };
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
        }

        public void SaveAccounts()
        {
            _accountService.WriteAllAccounts(Accounts.ToList());
        }

        public void NotifyChange()
        {
            AccountsChanged.Invoke();
        }
    }
}
