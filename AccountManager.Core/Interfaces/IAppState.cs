using AccountManager.Core.Models;
using AccountManager.Core.Services;

namespace AccountManager.Core.Interfaces
{
    public interface IAppState
    {
        List<Account> Accounts { get; set; }
        bool IsInitialized { get; set; }

        Task IpcLogin(AppState.IpcLoginParameter loginParam);
        Task SaveAccountOrder();
        void StartUpdateTimer();
        Task UpdateAccounts();
    }
}