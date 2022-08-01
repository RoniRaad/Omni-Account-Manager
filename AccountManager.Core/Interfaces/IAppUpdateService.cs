namespace AccountManager.Infrastructure.Services
{
    public interface IAppUpdateService
    {
        Task<bool> CheckForUpdate();
        Task Update();
        void Restart();
    }
}