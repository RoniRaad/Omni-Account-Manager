using AccountManager.Core.Enums;

namespace AccountManager.Core.Attributes
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public sealed class AccountTilePageAttribute : Attribute
    {
        public readonly int OrderNumber = 0;
        public readonly AccountType AccountType;

        public AccountTilePageAttribute(AccountType accountType, int orderNumber)
        {
            this.OrderNumber = orderNumber;
            this.AccountType = accountType;
        }
    }
}
