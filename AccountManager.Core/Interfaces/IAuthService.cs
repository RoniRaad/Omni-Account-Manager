namespace AccountManager.Core.Interfaces
{
    public interface IAuthService
    {
        bool AuthInitialized { get; set; }
        bool LoggedIn { get; set; }
        string PasswordHash { get; set; }

        Task ChangePasswordAsync(string oldPassword, string newPassword);
        Task LoginAsync(string password);
        Task RegisterAsync(string password);
    }
}