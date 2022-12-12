using AccountManager.Core.Enums;
using System.Text.Json.Serialization;

namespace AccountManager.Core.Models
{
    public sealed class Account
    {
        [JsonPropertyName("Guid")]
        public Guid Id { get; set; } = Guid.NewGuid();
        [JsonPropertyName("Id")]
        public string Name { get; set; }
        public string? PlatformId { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public AccountType AccountType { get; set; }
    }
    public sealed class LegacyAccount
    {
        public Guid Guid { get; set; } = Guid.NewGuid();
        public string Id { get; set; }
        public string? PlatformId { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public AccountType AccountType { get; set; }
    }
}
