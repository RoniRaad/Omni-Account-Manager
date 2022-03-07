using AccountManager.Core.Services;
using AccountManager.Core.Static;

namespace AccountManager.Core.ViewModels
{
    public class SettingsViewModel
    {
        private readonly AuthService _authService;
        private readonly AlertService _alertService;
        public SettingsViewModel(AuthService authService, AlertService alertService)
        {
            _authService = authService;
            _alertService = alertService;
        }

        public PasswordChangeRequest PasswordChangeRequest = new();
        public bool ShowChangePasswordPrompt = false;
        public void ChangePassword()
        {
            if (PasswordChangeRequest.NewPassword != PasswordChangeRequest.NewPasswordConfirm)
            {
                _alertService.ErrorMessage = "Error: New password fields do not match!";
                return;
            }
            if (StringEncryption.Hash(PasswordChangeRequest.OldPassword) != _authService.PasswordHash)
            {
                _alertService.ErrorMessage = "Error: Incorrect current password!";
                return;
            }

            _authService.ChangePassword(PasswordChangeRequest.OldPassword, PasswordChangeRequest.NewPassword);
            ToggleChangePassword();
            PasswordChangeRequest = new();
        }

        public void ToggleChangePassword()
        {
            ShowChangePasswordPrompt = !ShowChangePasswordPrompt;
        }
    }
    public class PasswordChangeRequest
    {
        public string OldPassword { get; set; }
        public string NewPassword { get; set; }
        public string NewPasswordConfirm { get; set; }
    }
}
