using AccountManager.Core.Interfaces;
using AccountManager.Core.Models;
using AccountManager.Core.Models.UserSettings;
using AccountManager.Infrastructure.Services;
using System.Security.Principal;
using System.Text.Json;

namespace AccountManager.Core.Services
{
    public sealed class AppState : IAppState
    {
        private readonly IAccountService _accountService;
        private readonly IUserSettingsService<Dictionary<Guid, AccountListItemSettings>> _accountItemSettings;
        public List<Account> Accounts { get; set; } = new();
        public bool IsInitialized { get; set; } = false;
        public AppState(IAccountService accountService, IIpcService ipcService, IUserSettingsService<Dictionary<Guid, AccountListItemSettings>> accountItemSettings)
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
            _accountItemSettings = accountItemSettings;
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
            var accounts = await _accountService.GetAllAccountsAsync();
            accounts = accounts.OrderBy((acc) =>
            {
                if (_accountItemSettings.Settings.TryGetValue(acc.Id, out var settings))
                    return settings.ListOrder;

                return 0;
            }
            ).ToList();

            Accounts = accounts;
            IsInitialized = true;                               
        }

        public async Task SaveAccountOrder()
        {
            for (int i = 0; i < Accounts.Count; i++)
            {
                if (_accountItemSettings.Settings.TryGetValue(Accounts[i].Id, out var settings))
                {
                    settings.ListOrder = i;
                }
            }

            await _accountItemSettings.SaveAsync();
        }

        public class IpcLoginParameter
        {
            public Guid Guid { get; set; }
        }
    }
}
