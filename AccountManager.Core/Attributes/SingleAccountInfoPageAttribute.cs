using AccountManager.Core.Enums;

namespace AccountManager.Core.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public class SingleAccountInfoPanelAttribute : Attribute
    {
        public readonly AccountType AccountType;

        public SingleAccountInfoPanelAttribute(AccountType accountType)
        {
            this.AccountType = accountType;
        }
    }
}
