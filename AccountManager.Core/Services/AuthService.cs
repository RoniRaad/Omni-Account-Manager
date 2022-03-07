using AccountManager.Core.Interfaces;
using AccountManager.Core.Static;
using AccountManager.Core.ViewModels;

namespace AccountManager.Core.Services
{
    public class AuthService
    {
        private IIOService _iOService;
        public string PasswordHash { get; set; } = "";
        public bool LoggedIn { get; set; }
        public bool AuthInitialized { get; set; }
        public Action UpdateMainView { get; set; }
        public AuthService(IIOService iOService)
        {
            _iOService = iOService;
            AuthInitialized = _iOService.ValidateData();
        }

        public void Login(string password)
        {
            PasswordHash = StringEncryption.Hash(password);
            LoggedIn = _iOService.TryLogin(PasswordHash);
        }

        public void Register(string password)
        {
            PasswordHash = StringEncryption.Hash(password);
            _iOService.UpdateData<List<object>>(new() ,PasswordHash);
            LoggedIn = true;
        }

        public void ChangePassword(string oldPassword, string newPassword)
        {
            if (!_iOService.TryLogin(PasswordHash))
                return;

            oldPassword = StringEncryption.Hash(oldPassword);
            newPassword = StringEncryption.Hash(newPassword);

            var currentData = _iOService.ReadData<List<AccountListItemViewModel>>(oldPassword);
            _iOService.UpdateData(currentData, newPassword);
            PasswordHash = newPassword;
        }
    }
}
