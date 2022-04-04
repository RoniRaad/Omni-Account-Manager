using AccountManager.Core.Models;
using System.Collections.ObjectModel;

namespace AccountManager.Core.Services
{
    public class AlertService
    {
        private readonly List<TwoFactorAuthenticationUserRequest> twoFactorRequests = new();
        private readonly List<string> errorMessages = new();
        private readonly List<string> infoMessages = new();
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

        public IEnumerable<string> GetErrorMessages()
        {
            return errorMessages;
        }

        public IEnumerable<string> GetInfoMessages()
        {
            return infoMessages;
        }

        public void AddErrorMessage(string errorMessage)
        {
            errorMessages.Add(errorMessage);
            Notify.Invoke();
            Task.Run(async () =>
            {
                await Task.Delay(6000);
                RemoveErrorMessage(errorMessage);
            }); 
        }

        public void AddInfoMessage(string infoMessage)
        {
            infoMessages.Add(infoMessage);
            Notify.Invoke();
        }

        public void RemoveErrorMessage(string errorMessage)
        {
            errorMessages.Remove(errorMessage);
            Notify.Invoke();
        }

        public void RemoveInfoMessage(string infoMessage)
        {
            infoMessages.Remove(infoMessage);
            Notify.Invoke();
        }

        public IEnumerable<TwoFactorAuthenticationUserRequest> GetTwoFactorAuthRequests()
        {
            return twoFactorRequests;
        }

    }

    public class TwoFactorAuthenticationUserRequest
    {
        public Account? Account { get; set; }
        public string EmailHint { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public Action<string>? Callback { get; set; }
    }
}
