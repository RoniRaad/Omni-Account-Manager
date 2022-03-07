namespace AccountManager.Core.Interfaces
{
    public interface IIOService
    {
        DriveInfo FindSteamDrive();
        string GetEncryptedUsername();
        List<string[]> GetInstalledGamesManifest();
        T ReadData<T>(string password) where T : new();
        T ReadData<T>() where T : new();
        bool TryLogin(string password);
        void UpdateData<T>(T data, string password);
        void UpdateData<T>(T data);
        bool ValidateData();
    }
}