namespace AccountManager.Core.Models.UserSettings
{
    public class AccountListItemSettings
    {
        public Guid AccountGuid { get; set; } = Guid.Empty;
        public bool ShowAccountDetails { get; set; } = true;
    }
}