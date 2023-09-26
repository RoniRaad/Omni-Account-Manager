namespace AccountManager.Core.Interfaces
{
    public interface IAuthService
    {
        bool AuthInitialized { get; set; }
        bool LoggedIn { get; set; }
        string PasswordHash { get; set; }

        Task ChangePasswordAsync(string oldPassword, string newPassword);
        Task<bool> LoginAsync(string password);
        Task<bool> RegisterAsync(string password);
    }
}