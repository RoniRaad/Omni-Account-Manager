﻿using AccountManager.Core.Enums;
using AccountManager.Core.Interfaces;
using AccountManager.Core.Models;
using System.Text.Json.Serialization;

namespace AccountManager.Core.ViewModels
{
    public class AccountListItemViewModel : IAccountListItemViewModel
    {
        public string Name { get; set; } = string.Empty;
        public Rank Rank { get; set; }
        public bool IsEditing = false;
        public Account Account { get; set; }
        public AccountType AccountType { get; set; }
        [JsonIgnore]
        public IPlatformService PlatformService { get; set; }
        [JsonIgnore]
        public Action Delete { get; set; }
        public async Task ToggleEdit()
        {
            IsEditing = !IsEditing;
        }
        public async Task Login()
        {
            Task.Factory.StartNew(() => PlatformService.Login(Account).ConfigureAwait(false));
        }
    }
}
