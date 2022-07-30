using AccountManager.Core.Models;

namespace AccountManager.Core.Interfaces
{
    public interface IUserSettingsService<T> where T : new()
    {
        T Settings { get; set; }

        event Action OnSettingsSaved;

        bool ChangePassword(PasswordChangeRequest changeRequest);
        void ClearCookies();
        void Save();
    }
}