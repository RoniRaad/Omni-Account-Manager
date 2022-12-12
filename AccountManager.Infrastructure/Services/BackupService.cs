using AccountManager.Core.Interfaces;
using AccountManager.Infrastructure.Services.FileSystem;

namespace AccountManager.Infrastructure.Services
{
    public class BackupService : IBackupService
    {
        private static string BackupFolder = Path.Combine(GeneralFileSystemService.DataPath, "Backups");
        
        public async Task CreateBackup()
        {
            if (!Directory.Exists(BackupFolder))
                Directory.CreateDirectory(BackupFolder);

            var accountDatabaseFilePath = Path.Combine(GeneralFileSystemService.DataPath, "accounts.db");
            var newBackupFileName = $"accounts-{DateTime.Now.ToString("yyyy-dd-M")}.db";
            var newBackupFilePath = Path.Combine(BackupFolder, newBackupFileName);

            if (File.Exists(newBackupFilePath))
                return;

            var currentFileContents = await File.ReadAllBytesAsync(accountDatabaseFilePath);
            await File.WriteAllBytesAsync(newBackupFilePath, currentFileContents);
        }

        public void ClearOldBackups()
        {
            if (!Directory.Exists(BackupFolder))
                return;

            foreach (var file in Directory.GetFiles(BackupFolder))
            {
                var creationTime = File.GetCreationTime(file);
                var fileName = Path.GetFileNameWithoutExtension(file);
                if (creationTime < DateTime.Now.AddDays(-5) && fileName.StartsWith("accounts-"))
                {
                    File.Delete(file);
                }
            }
        }
    }
}
