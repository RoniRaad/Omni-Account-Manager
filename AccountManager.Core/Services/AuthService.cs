using AccountManager.Core.Interfaces;
using AccountManager.Core.Models;
using AccountManager.Core.Static;
using Microsoft.Extensions.Caching.Distributed;

namespace AccountManager.Core.Services
{
    public sealed class SqliteAuthService : IAuthService
    {
        private readonly IAccountEncryptedRepository _accountRepository;
        private readonly IAlertService _alertService;
        private readonly IDistributedCache _persistantCache;
        public string PasswordHash { get; set; } = "";
        public bool LoggedIn { get; set; }
        public bool AuthInitialized { get; set; } = true;
        public SqliteAuthService(IAccountEncryptedRepository accountRepository, IAlertService alertService, IDistributedCache persistantCache)
        {
            _accountRepository = accountRepository;
            _alertService = alertService;
            _persistantCache = persistantCache;
        }

        public async Task LoginAsync(string password)
        {
            PasswordHash = StringEncryption.Hash(password);
            LoggedIn = _accountRepository.TryDecrypt(PasswordHash);
            if (!LoggedIn)
            {
                _alertService.AddErrorAlert("Error incorrect password!");
            }

            if (await _persistantCache.GetAsync<bool>(CacheKeys.LoginCacheKeys.RememberMe))
                await _persistantCache.SetAsync(CacheKeys.LoginCacheKeys.RememberedPassword, password);
        }

        public async Task RegisterAsync(string password)
        {
            PasswordHash = StringEncryption.Hash(password);
            LoggedIn = true;
        }

        public async Task ChangePasswordAsync(string oldPassword, string newPassword)
        {
            oldPassword = StringEncryption.Hash(oldPassword);
            newPassword = StringEncryption.Hash(newPassword);

            if (string.IsNullOrEmpty(oldPassword) || string.IsNullOrEmpty(newPassword) || !_accountRepository.TryDecrypt(oldPassword))
                return;

            if (_accountRepository.TryChangePassword(oldPassword, newPassword))
                PasswordHash = newPassword;
        }
    }
}
