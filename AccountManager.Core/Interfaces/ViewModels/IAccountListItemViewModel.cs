using AccountManager.Core.Enums;
using AccountManager.Core.Interfaces;
using AccountManager.Core.Models;

namespace AccountManager.Core.ViewModels
{
    public interface IAccountListItemViewModel
    {
        Account Account { get; set; }
        AccountType AccountType { get; set; }
        Action Delete { get; set; }
        IPlatformService PlatformService { get; set; }
        string Name { get; set; }
        Rank Rank { get; set; }

        Task Login();
        Task ToggleEdit();
    }
}