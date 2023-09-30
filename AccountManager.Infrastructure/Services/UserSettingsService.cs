using AccountManager.Core.Interfaces;
using AccountManager.Core.Models;
using AccountManager.Core.Static;

namespace AccountManager.Infrastructure.Services
{
    public sealed class UserSettingsService<T> : IUserSettingsService<T> where T : new()
    {
        public T Settings { get; set; }
        public event Action OnSettingsSaved = delegate { };
        private readonly IGeneralFileSystemService _iOService;
        private readonly IAuthService _authService;
        private readonly IAlertService _alertService;
        public UserSettingsService(IGeneralFileSystemService iOService, IAuthService authService, IAlertService alertService)
        {
            _iOService = iOService;
            Settings = _iOService.ReadData<T>();
            _authService = authService;
            _alertService = alertService;
        }

        public async Task SaveAsync() {
            await _iOService.WriteDataAsync(Settings);
            OnSettingsSaved.Invoke();
        }

        public bool ChangePassword(PasswordChangeRequest changeRequest)
        {
            if (changeRequest.NewPassword != changeRequest.NewPasswordConfirm)
            {
                _alertService.AddErrorAlert("Error: New password fields do not match!");
                return false;
            }
            if (StringEncryption.Hash(changeRequest.OldPassword) != _authService.PasswordHash)
            {
                _alertService.AddErrorAlert("Error: Incorrect current password!");
                return false;
            }

            _authService.ChangePassword(changeRequest.OldPassword, changeRequest.NewPassword);
            _alertService.AddInfoAlert("Password changed successfully!");
            return true;
        }

        public void ClearCookies()
        {
            _iOService.AddCacheDeleteFlag();
            Environment.Exit(0);
        }
    }
}
