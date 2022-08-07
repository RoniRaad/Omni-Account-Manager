namespace AccountManager.Core.Interfaces
{
    public interface IAuthService
    {
        bool AuthInitialized { get; set; }
        bool LoggedIn { get; set; }
        string PasswordHash { get; set; }

        void ChangePassword(string oldPassword, string newPassword);
        void Login(string password);
        void Register(string password);
    }
}