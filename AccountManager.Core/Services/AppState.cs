using AccountManager.Core.Interfaces;
using AccountManager.Core.Models;
using AccountManager.Core.Static;
using System.Collections.ObjectModel;

namespace AccountManager.Core.Services
{
    public class AppState
    {
        private readonly IAccountService _accountService;
        public ObservableCollection<Account> Accounts { get; set; }
        public event Action Notify;
        public AppState(IAccountService accountService)
        {
            _accountService = accountService;
            Accounts = new ObservableCollection<Account>(_accountService.GetAllAccountsMin());
            Notify = delegate
            {

            };
            Accounts.CollectionChanged += async (s, e) => {
                Accounts?.RemoveAll((account) => Accounts.Count((innerAccount) => account.Guid == innerAccount.Guid) > 1);

                if (Accounts is not null)
                    _accountService.WriteAllAccounts(Accounts?.ToList());

                await Task.Delay(1); // Fixes UI bugs with elements dependent on the Accounts property
                Notify.Invoke();
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
            Notify.Invoke();
        }
    }
}
