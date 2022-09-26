﻿using AccountManager.Core.Enums;

namespace AccountManager.Core.Models
{
    public sealed class Account
    {
        public Guid Guid { get; set; } = Guid.NewGuid();
        public string? Id { get; set; } // Non-Unique ID
        public string? PlatformId { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public Rank Rank { get; set; } = new();
        public AccountType AccountType { get; set; }
    }
}
