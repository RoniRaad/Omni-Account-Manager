using AccountManager.Core.Models;
using System.Collections.ObjectModel;

namespace AccountManager.Core.Services
{
    public class AlertService
    {
        public TwoFactorAuthenticationUserRequest? TwoFactorRequest = null;
        public ObservableCollection<TwoFactorAuthenticationUserRequest> TwoFactorRequests = new();

        private string errorMessage = "";
        private string infoMessage = "";
        private bool twoFactorPrompt = false;

        public bool TwoFactorPrompt
        {
            get
            {
                return this.twoFactorPrompt;
            }
            set
            {
                twoFactorPrompt = value;
                if (UpdateView is not null)
                    UpdateView();
            }
        }

        public Action? UpdateView { get; set; }
        public string ErrorMessage
        {
            get
            {
                return this.errorMessage;
            }
            set
            {
                errorMessage = value;
                if (UpdateView is not null)
                    UpdateView();
            }
        }
        public string InfoMessage
        {
            get
            {
                return this.infoMessage;
            }
            set
            {
                infoMessage = value;
                if (UpdateView is not null)
                    UpdateView();
            }
        }

        public async Task<string> PromptUserFor2FA(Account account, string emailHint)
        {
            string? returnCode = null;
            Action<string>? callback = null;
            var request = new TwoFactorAuthenticationUserRequest()
            {
                Account = account,
                EmailHint = emailHint,
                Callback = (code) => returnCode = code
            };
            callback = (code) =>
            {
                returnCode = code;
            };

            TwoFactorRequests.Add(request);
            if (UpdateView is not null)
                UpdateView();

            while (returnCode is null)
            {
                await Task.Delay(100);
            }

            TwoFactorRequests.Remove(request);
            if (UpdateView is not null)
                UpdateView();

            return returnCode;
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
