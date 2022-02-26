using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AccountManager.Core.Services
{
    public class AuthService
    {
        private IIOService _iOService;
        public string PasswordHash { get; set; } = "";
        public bool LoggedIn { get; set; }
        public bool AuthInitialized { get; set; }
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

            var currentData = _iOService.ReadDataAsString(oldPassword);
            _iOService.WriteDataAsString(newPassword, currentData);
            PasswordHash = newPassword;
        }
    }
}
