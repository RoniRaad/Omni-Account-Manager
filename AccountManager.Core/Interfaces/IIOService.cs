namespace AccountManager.Core.Services
{
    public interface IIOService
    {
        DriveInfo FindSteamDrive();
        string GetConfig();
        string GetEncryptedUsername();
        List<string[]> GetInstalledGamesManifest();
        void InitializeData(string password);
        T ReadData<T>(string password) where T : new();
        void SaveConfig(string contents);
        bool TryLogin(string password);
        void UpdateData<T>(T data, string password);
        bool ValidateData();
    }
}