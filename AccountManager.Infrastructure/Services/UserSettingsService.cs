using AccountManager.Core.Enums;
using AccountManager.Core.Interfaces;
using AccountManager.Core.Models;
using AccountManager.Core.Services;
using AccountManager.Core.Static;
using AccountManager.Infrastructure.Services.FileSystem;

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

        class LegacyAccount
        {
            public string Name { get; set; } = string.Empty;
            public Rank Rank { get; set; }
            public bool IsEditing = false;
            public Account Account { get; set; }
            public AccountType AccountType { get; set; }
        }

        public void Transfer()
        {

            var readAsAccount = _iOService.ReadData<List<Account>>(_authService.PasswordHash);
            var readAsLegacy = _iOService.ReadData<List<LegacyAccount>>(_authService.PasswordHash);
            for (int i = 0; i < readAsAccount.Count; i++)
            {
                if (readAsLegacy[i].Name is not null)
                    readAsAccount[i].Id = readAsLegacy[i].Name;
                if (readAsLegacy[i]?.Account?.Username is not null)
                    readAsAccount[i].Username = readAsLegacy[i].Account.Username;
                if (readAsLegacy[i]?.Account?.Password is not null)
                    readAsAccount[i].Password = readAsLegacy[i].Account.Password;
                if (readAsLegacy[i]?.Account?.AccountType is not null)
                    readAsAccount[i].AccountType = readAsLegacy[i].Account.AccountType;
                if (readAsLegacy[i]?.Account?.PlatformId is not null)
                    readAsAccount[i].PlatformId = readAsLegacy[i].Account.PlatformId;
                if (readAsLegacy[i]?.Account?.Rank is not null)
                    readAsAccount[i].Rank = readAsLegacy[i].Account.Rank;
            }

            _iOService.UpdateData(readAsAccount, _authService.PasswordHash);
        }
    }
}
