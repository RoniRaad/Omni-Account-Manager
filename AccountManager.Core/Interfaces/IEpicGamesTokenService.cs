namespace AccountManager.Core.Interfaces
{
    public interface IEpicGamesTokenService
    {
        void CloseBrowser();
        Task TrySignIn(string username, string password);
    }
}