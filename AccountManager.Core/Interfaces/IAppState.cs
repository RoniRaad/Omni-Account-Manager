using AccountManager.Core.Models;
using AccountManager.Core.Services;

namespace AccountManager.Core.Interfaces
{
    public interface IAppState
    {
        List<Account> Accounts { get; set; }
        bool IsInitialized { get; set; }

        Task IpcLogin(AppState.IpcLoginParameter loginParam);
        void SaveAccounts();
        void StartUpdateTimer();
        Task UpdateAccounts();
    }
}