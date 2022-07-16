using AccountManager.Core.Interfaces;
using AccountManager.Core.Models;
using AccountManager.Core.Static;
using AccountManager.Infrastructure.Services;
using Microsoft.Extensions.Logging;
using System.Collections.ObjectModel;
using System.Text.Json;

namespace AccountManager.Core.Services
{
    public class AppState
    {
        private readonly IAccountService _accountService;
        public RangeObservableCollection<Account> Accounts { get; set; }
        public bool IsInitialized { get; set; } = false;
        public AppState(IAccountService accountService, IIpcService ipcService)
        {
            _accountService = accountService;
            Accounts = new RangeObservableCollection<Account>();
            Accounts.AddRange(_accountService.GetAllAccountsMin());
          
            _ = UpdateAccounts();

            StartUpdateTimer();

            ipcService.IpcReceived += (sender, args) =>
            {
                if (args.MethodName == nameof(IpcLogin) && args?.Json is not null)
                {
                    try
                    {
                        var param = JsonSerializer.Deserialize<IpcLoginParameter>(args.Json);

                        if (param is not null)
                            IpcLogin(param);
                    }
                    catch
                    {
                        // unable to deserialze IpcLogin attempt
                    }
                }
            };
        }

        public void IpcLogin(IpcLoginParameter loginParam)
        {
            var relevantAccount = Accounts.FirstOrDefault((account) => account.Guid == loginParam.Guid);

            if (relevantAccount is not null)
                _accountService.Login(relevantAccount);

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

        public class IpcLoginParameter
        {
            public Guid Guid { get; set; }
        }
    }
}
