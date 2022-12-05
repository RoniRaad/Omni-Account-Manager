using AccountManager.Core.Interfaces;
using AccountManager.Core.Models;
using AccountManager.Infrastructure.Services;
using System.Text.Json;

namespace AccountManager.Core.Services
{
    public sealed class AppState : IAppState
    {
        private readonly IAccountService _accountService;
        public List<Account> Accounts { get; set; }
        public bool IsInitialized { get; set; } = false;
        public AppState(IAccountService accountService, IIpcService ipcService)
        {
            _accountService = accountService;
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
            var relevantAccount = Accounts.FirstOrDefault((account) => account.Id == loginParam.Guid);

            if (relevantAccount is not null)
                await _accountService.LoginAsync(relevantAccount);
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
            Accounts = await _accountService.GetAllAccountsMinAsync();

            var fullAccounts = new List<Account>(await _accountService.GetAllAccountsAsync());
            for (int i = 0; i < Accounts.Count; i++)
            {
                Accounts[i].PlatformId = fullAccounts.FirstOrDefault((updatedAccount) => Accounts[i].Id == updatedAccount.Id)?.PlatformId ?? Accounts[i].PlatformId;
            }

            SaveAccounts();

            IsInitialized = true;
        }

        public void SaveAccounts()
        {
            _accountService.WriteAllAccountsAsync(Accounts);
        }

        public class IpcLoginParameter
        {
            public Guid Guid { get; set; }
        }
    }
}
