using AccountManager.Core.Interfaces;
using AccountManager.Core.Models;
using AccountManager.Core.Static;

namespace AccountManager.Core.Services
{
    public class AuthService
    {
        private readonly IIOService _iOService;
        private readonly AlertService _alertService;
        public string PasswordHash { get; set; } = "";
        public bool LoggedIn { get; set; }
        public bool AuthInitialized { get; set; }
        public AuthService(IIOService iOService, AlertService alertService)
        {
            _iOService = iOService;
            AuthInitialized = _iOService.ValidateData();
            _alertService = alertService;
        }

        public void Login(string password)
        {
            PasswordHash = StringEncryption.Hash(password);
            LoggedIn = _iOService.TryLogin(PasswordHash);
            if (!LoggedIn)
            {
                _alertService.AddErrorMessage("Error incorrect password!");
            }
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

            var currentData = _iOService.ReadData<List<Account>>(oldPassword);
            _iOService.UpdateData(currentData, newPassword);
            PasswordHash = newPassword;
        }
    }
}
