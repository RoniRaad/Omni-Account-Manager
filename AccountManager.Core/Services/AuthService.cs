using AccountManager.Core.Interfaces;
using AccountManager.Core.Models;
using AccountManager.Core.Static;
using Microsoft.Extensions.Caching.Distributed;

namespace AccountManager.Core.Services
{
    public class AuthService : IAuthService
    {
        private readonly IIOService _iOService;
        private readonly IAlertService _alertService;
        private readonly IDistributedCache _persistantCache;
        public string PasswordHash { get; set; } = "";
        public bool LoggedIn { get; set; }
        public bool AuthInitialized { get; set; }
        public AuthService(IIOService iOService, IAlertService alertService, IDistributedCache persistantCache)
        {
            _iOService = iOService;
            AuthInitialized = _iOService.ValidateData();
            _alertService = alertService;
            _persistantCache = persistantCache;
        }

        public void Login(string password)
        {
            PasswordHash = StringEncryption.Hash(password);
            LoggedIn = _iOService.TryReadEncryptedData(PasswordHash);
            if (!LoggedIn)
            {
                _alertService.AddErrorAlert("Error incorrect password!");
            }

            Task.Run(async () =>
            {
                if (await _persistantCache.GetAsync<bool>("rememberPassword"))
                    await _persistantCache.SetAsync("masterPassword", password);
            });
        }

        public void Register(string password)
        {
            PasswordHash = StringEncryption.Hash(password);
            _iOService.UpdateData<List<object>>(new(), PasswordHash);
            LoggedIn = true;
        }

        public void ChangePassword(string oldPassword, string newPassword)
        {
            if (string.IsNullOrEmpty(oldPassword) || string.IsNullOrEmpty(newPassword) || !_iOService.TryReadEncryptedData(oldPassword))
                return;

            oldPassword = StringEncryption.Hash(oldPassword);
            newPassword = StringEncryption.Hash(newPassword);

            var currentData = _iOService.ReadData<List<Account>>(oldPassword);
            _iOService.UpdateData(currentData, newPassword);
            PasswordHash = newPassword;
        }
    }
}
