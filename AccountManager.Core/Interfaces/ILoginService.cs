using AccountManager.Core.Models;

namespace AccountManager.Core.Interfaces
{
    public interface ILoginService
    {
        Task Login(Account account);
    }
}