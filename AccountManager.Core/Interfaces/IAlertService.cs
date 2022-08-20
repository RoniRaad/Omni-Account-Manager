using AccountManager.Core.Models;
using AccountManager.Core.Services;

namespace AccountManager.Core.Interfaces
{
    public interface IAlertService
    {
        event Action Notify;

        void AddErrorAlert(string errorMessage);
        void AddInfoAlert(string infoMessage);
        IEnumerable<Alert> GetErrorAlerts();
        IEnumerable<Alert> GetInfoAlerts();
        IEnumerable<TwoFactorAuthenticationUserRequest> GetTwoFactorAuthRequests();
        Task<string> PromptUserFor2FA(Account account, string emailHint);
        void RemoveErrorMessage(Alert errorMessage);
        void RemoveInfoMessage(Alert infoMessage);
    }
}