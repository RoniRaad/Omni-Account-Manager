namespace AccountManager.Core.Models.UserSettings
{
    public sealed class AccountListItemSettings
    {
        public Guid AccountGuid { get; set; } = Guid.Empty;
        public bool ShowAccountDetails { get; set; } = true;
        public int ListOrder { get; set; } = int.MaxValue;
    }
}