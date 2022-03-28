using AccountManager.Core.Interfaces;
using AccountManager.Core.Models;
using AccountManager.Core.Services;
using AccountManager.Core.Static;

namespace AccountManager.Infrastructure.Services
{
    public class UserSettingsService<T> : IUserSettingsService<T> where T : new()
    {
        public T Settings { get; set; }
        private readonly IIOService _iOService;
        private readonly AuthService _authService;
        private readonly AlertService _alertService;
        public UserSettingsService(IIOService iOService, AuthService authService, AlertService alertService)
        {
            _iOService = iOService;
            Settings = _iOService.ReadData<T>();
            _authService = authService;
            _alertService = alertService;
        }

        public void Save() => _iOService.UpdateData(Settings);

        public bool ChangePassword(PasswordChangeRequest changeRequest)
        {
            if (changeRequest.NewPassword != changeRequest.NewPasswordConfirm)
            {
                _alertService.ErrorMessage = "Error: New password fields do not match!";
                return false;
            }
            if (StringEncryption.Hash(changeRequest.OldPassword) != _authService.PasswordHash)
            {
                _alertService.ErrorMessage = "Error: Incorrect current password!";
                return false;
            }

            _authService.ChangePassword(changeRequest.OldPassword, changeRequest.NewPassword);
            return true;
        }
    }
}
