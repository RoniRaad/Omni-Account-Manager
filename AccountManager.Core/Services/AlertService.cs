using AccountManager.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AccountManager.Core.Services
{
    public class AlertService
    {
        public TwoFactorAuthenticationUserRequest? TwoFactorRequest = null;
        public event EventHandler TwoFactorRequestSubmitted;
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
                UpdateView();
            }
        }

        public Action UpdateView { get; set; }
        public string ErrorMessage
        {
            get
            {
                return this.errorMessage;
            }
            set
            {
                errorMessage = value;
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
                UpdateView();
            }
        }

        public async Task<string> PromptUserFor2FA(Account account)
        {
            var code = "";
            TwoFactorRequest = new()
            {
                Username = account.Username
            };
            TwoFactorPrompt = true;
            TwoFactorRequestSubmitted += delegate
            {
                code = TwoFactorRequest.Code;
            };

            while (TwoFactorPrompt)
            {
                await Task.Delay(100);
            }

            return code;
        }

        public void Cancel2FA()
        {
            TwoFactorRequest.Code = "";
            Submit2FA();
        }

        public void Submit2FA()
        {
            TwoFactorRequestSubmitted?.Invoke(this, EventArgs.Empty);
            TwoFactorRequestSubmitted = null;
            TwoFactorPrompt = false;
        }
    }
    public class TwoFactorAuthenticationUserRequest
    {
        public string Username { get; set; }
        public string EmailHint { get; set; }
        public string Code { get; set; }
        public bool Cancel { get; set; }
    }
}
