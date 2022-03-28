using AccountManager.Core.Models;

namespace AccountManager.Core.Interfaces
{
    public interface IUserSettingsService<T> where T : new()
    {
        T Settings { get; set; }

        bool ChangePassword(PasswordChangeRequest newPassword);

        void Save();
    }
}