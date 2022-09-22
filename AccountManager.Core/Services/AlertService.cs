using AccountManager.Core.Interfaces;
using AccountManager.Core.Models;

namespace AccountManager.Core.Services
{
    public sealed class AlertService : IAlertService
    {
        private readonly List<TwoFactorAuthenticationUserRequest> twoFactorRequests = new();
        private readonly List<Alert> errorMessages = new();
        private readonly List<Alert> infoMessages = new();
        public event Action Notify = delegate { };
        public async Task<string> PromptUserFor2FA(Account account, string emailHint)
        {
            string? returnCode = null;
            var request = new TwoFactorAuthenticationUserRequest()
            {
                Account = account,
                EmailHint = emailHint,
                Callback = (code) => returnCode = code
            };

            twoFactorRequests.Add(request);
            Notify.Invoke();

            while (returnCode is null)
            {
                await Task.Delay(100);
            }

            twoFactorRequests.Remove(request);
            Notify.Invoke();

            return returnCode;
        }

        public IEnumerable<Alert> GetErrorAlerts()
        {
            return errorMessages;
        }

        public IEnumerable<Alert> GetInfoAlerts()
        {
            return infoMessages;
        }

        public void AddErrorAlert(string errorMessage)
        {
            if (string.IsNullOrEmpty(errorMessage))
                return;

            errorMessages.Add(new Alert { DisplayMessage = errorMessage, Type = AlertType.Error } );
            Notify.Invoke();
        }

        public void AddInfoAlert(string infoMessage)
        {
            if (string.IsNullOrEmpty(infoMessage))
                return;


            infoMessages.Add(new Alert { DisplayMessage = infoMessage, Type = AlertType.Info });
            Notify.Invoke();
        }

        public void RemoveErrorMessage(Alert errorMessage)
        {
            errorMessages.Remove(errorMessage);
            Notify.Invoke();
        }

        public void RemoveInfoMessage(Alert infoMessage)
        {
            infoMessages.Remove(infoMessage);
            Notify.Invoke();
        }

        public IEnumerable<TwoFactorAuthenticationUserRequest> GetTwoFactorAuthRequests()
        {
            return twoFactorRequests;
        }
    }

    public sealed class TwoFactorAuthenticationUserRequest
    {
        public Account? Account { get; set; }
        public string EmailHint { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public Action<string> Callback { get; set; } = delegate { };
    }

    public sealed class Alert
    {
        public AlertType Type { get; set; }
        public string DisplayMessage { get; set; } = "";
        public DateTime CreateTime { get; set; } = DateTime.Now;
    }

    public enum AlertType
    {
        Error,
        Info
    }
}
