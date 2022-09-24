using AccountManager.Core.Models;

namespace AccountManager.Core.Interfaces
{
    public interface IUserSettingsService<T> where T : new()
    {
        T Settings { get; set; }

        event Action OnSettingsSaved;

        Task<bool> ChangePasswordAsync(PasswordChangeRequest changeRequest);
        void ClearCookies();
        Task SaveAsync();
    }
}