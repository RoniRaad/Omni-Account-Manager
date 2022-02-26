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
        public AuthService(IIOService iOService)
        {
            _iOService = iOService;
        }
        public string PasswordHash { get; set; } = "";
        public bool LoggedIn { get; set; }
        public void Login(string password)
        {
            PasswordHash = StringEncryption.Hash(password);
            LoggedIn = _iOService.TryLogin(PasswordHash);
        }
    }
}
