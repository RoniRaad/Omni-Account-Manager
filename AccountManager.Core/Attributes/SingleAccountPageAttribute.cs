using AccountManager.Core.Enums;

namespace AccountManager.Core.Attributes
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public sealed class SingleAccountPageAttribute : Attribute
    {
        public readonly string Title;
        public readonly int OrderNumber = 0;
        public readonly AccountType AccountType;

        public SingleAccountPageAttribute(string title, AccountType accountType, int orderNumber)
        {
            this.Title = title;
            this.OrderNumber = orderNumber;
            this.AccountType = accountType;
        }

        public SingleAccountPageAttribute(string title, AccountType accountType)
        {
            this.Title = title;
            this.AccountType = accountType;
        }
    }
}
