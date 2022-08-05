using AccountManager.Core.Interfaces;
using AccountManager.Core.Models;
using AccountManager.Core.Static;
using AccountManager.Infrastructure.Services;
using System.Collections.ObjectModel;
using System.Text.Json;
using System.Linq;

namespace AccountManager.Core.Services
{
    public class AppState : IAppState
    {
        private readonly IAccountService _accountService;
        public List<Account> Accounts { get; set; }
        public bool IsInitialized { get; set; } = false;
        public AppState(IAccountService accountService, IIpcService ipcService)
        {
            _accountService = accountService;
            Accounts = _accountService.GetAllAccountsMin();

            Task.Run(UpdateAccounts);

            StartUpdateTimer();

            ipcService.IpcReceived += (sender, args) =>
            {
                if (args.MethodName == nameof(IpcLogin) && args?.Json is not null)
                {
                    try
                    {
                        var param = JsonSerializer.Deserialize<IpcLoginParameter>(args.Json);

                        if (param is not null)
                            Task.Run(() => IpcLogin(param));
                    }
                    catch
                    {
                        // unable to deserialze IpcLogin attempt
                    }
                }
            };
        }

        public async Task IpcLogin(IpcLoginParameter loginParam)
        {
            var relevantAccount = Accounts.FirstOrDefault((account) => account.Guid == loginParam.Guid);

            if (relevantAccount is not null)
                await _accountService.Login(relevantAccount);
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

            var fullAccounts = new List<Account>(await _accountService.GetAllAccounts());

            Accounts.ForEach((currentAccount) => {
                currentAccount.Rank = fullAccounts.FirstOrDefault((updatedAccount) => currentAccount.Guid == updatedAccount.Guid)?.Rank ?? currentAccount.Rank;
                currentAccount.PlatformId = fullAccounts.FirstOrDefault((updatedAccount) => currentAccount.Guid == updatedAccount.Guid)?.PlatformId ?? currentAccount.PlatformId;
            });

            SaveAccounts();

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
