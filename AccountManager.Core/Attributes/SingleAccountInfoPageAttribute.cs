using AccountManager.Core.Enums;

namespace AccountManager.Core.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class SingleAccountInfoPanelAttribute : Attribute
    {
        public readonly AccountType AccountType;

        public SingleAccountInfoPanelAttribute(AccountType accountType)
        {
            this.AccountType = accountType;
        }
    }
}
