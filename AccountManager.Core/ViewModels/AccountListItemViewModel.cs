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
        public string Name { get; set; }
        public Account Account { get; set; }
        public AccountType AccountType { get; set; }
        [JsonIgnore]
        public ILoginService LoginService { get; set; }
        [JsonIgnore]
        public Action Delete { get; set; }
    }
}
