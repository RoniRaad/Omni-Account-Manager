namespace AccountManager.Core.Interfaces
{
    public interface IIOService
    {
        bool IsFileLocked(string filePath);
        T ReadData<T>(string password) where T : new();
        T ReadData<T>() where T : new();
        bool TryLogin(string password);
        void UpdateData<T>(T data, string password);
        void UpdateData<T>(T data);
        bool ValidateData();
        void AddCacheDeleteFlag();
    }
}