using AccountManager.Core.Enums;
using AccountManager.Core.Interfaces;
using AccountManager.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace AccountManager.Core.ViewModels
{
    public class AccountListItemViewModel
    {
        public string Name { get; set; } = string.Empty;
        public bool IsEditing = false;
        public Account Account { get; set; }
        public AccountType AccountType { get; set; }
        [JsonIgnore]
        public ILoginService LoginService { get; set; }
        [JsonIgnore]
        public Action Delete { get; set; }
        public async Task ToggleEdit()
        {
            IsEditing = !IsEditing;
        }
        public async Task Login()
        {
            Task.Factory.StartNew(() => LoginService.Login(Account).ConfigureAwait(false));
        }
    }
}
