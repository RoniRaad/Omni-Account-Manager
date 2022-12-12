namespace AccountManager.Core.Interfaces
{
    public interface IBackupService
    {
        void ClearOldBackups();
        Task CreateBackup();
    }
}