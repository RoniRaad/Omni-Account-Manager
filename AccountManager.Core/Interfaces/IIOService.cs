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
        string ReadDataAsString(string password);
        void SaveConfig(string contents);
        bool TryLogin(string password);
        void UpdateData<T>(T data, string password);
        bool ValidateData();
        void WriteDataAsString(string password, string data);
    }
}