using System;
using System.Text.Json.Serialization;

namespace AccountManager.UI.Services
{
    public sealed class AccountInfo
    {
        [JsonPropertyName("id")]
        public string? Id { get; set; }

        [JsonPropertyName("displayName")]
        public string? DisplayName { get; set; }

        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("email")]
        public string? Email { get; set; }

        [JsonPropertyName("failedLoginAttempts")]
        public int FailedLoginAttempts { get; set; }

        [JsonPropertyName("lastLogin")]
        public DateTime LastLogin { get; set; }

        [JsonPropertyName("numberOfDisplayNameChanges")]
        public int NumberOfDisplayNameChanges { get; set; }

        [JsonPropertyName("ageGroup")]
        public string? AgeGroup { get; set; }

        [JsonPropertyName("headless")]
        public bool Headless { get; set; }

        [JsonPropertyName("country")]
        public string? Country { get; set; }

        [JsonPropertyName("lastName")]
        public string? LastName { get; set; }

        [JsonPropertyName("preferredLanguage")]
        public string? PreferredLanguage { get; set; }

        [JsonPropertyName("canUpdateDisplayName")]
        public bool CanUpdateDisplayName { get; set; }

        [JsonPropertyName("tfaEnabled")]
        public bool TfaEnabled { get; set; }

        [JsonPropertyName("emailVerified")]
        public bool EmailVerified { get; set; }

        [JsonPropertyName("minorVerified")]
        public bool MinorVerified { get; set; }

        [JsonPropertyName("minorExpected")]
        public bool MinorExpected { get; set; }

        [JsonPropertyName("minorStatus")]
        public string? MinorStatus { get; set; }

        [JsonPropertyName("cabinedMode")]
        public bool CabinedMode { get; set; }
    }
}