using AccountManager.Core.Models;
using AccountManager.Core.Services;

namespace AccountManager.Core.Interfaces
{
    public interface IAppState
    {
        RangeObservableCollection<Account> Accounts { get; set; }
        bool IsInitialized { get; set; }

        void IpcLogin(AppState.IpcLoginParameter loginParam);
        void SaveAccounts();
        void StartUpdateTimer();
        Task UpdateAccounts();
    }
}