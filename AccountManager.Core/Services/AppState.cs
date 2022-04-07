using AccountManager.Core.Interfaces;
using AccountManager.Core.Models;

namespace AccountManager.Core.Services
{
    public class AppState
    {
        private readonly IAccountService _accountService;
        public List<Account> Accounts { get; set; }
        public event Action Notify;
        public AppState(IAccountService accountService)
        {
            _accountService = accountService;
            Accounts = _accountService.GetAllAccountsMin();

            _ = UpdateAccounts();

            Notify = delegate
            {

            };

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
            foreach (var account in Accounts)
            {
                var relevantAccount = minAccounts.FirstOrDefault((acc) => acc.Guid == account.Guid, null);
                if (relevantAccount is null)
                {
                    Accounts.Remove(account);
                }
                else
                {
                    minAccounts.Remove(relevantAccount);
                }
            }
            Accounts.AddRange(minAccounts);
            Notify.Invoke();

            var fullAccounts = await _accountService.GetAllAccounts();
            Accounts = fullAccounts;
            Notify.Invoke();
        }

        public void SaveAccounts()
        {
            _accountService.WriteAllAccounts(Accounts);
        }
    }
}
