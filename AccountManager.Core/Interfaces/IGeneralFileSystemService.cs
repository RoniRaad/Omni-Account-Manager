namespace AccountManager.Core.Interfaces
{
    public interface IGeneralFileSystemService
    {
        bool IsFileLocked(string filePath);
        T ReadData<T>(string password) where T : new();
        T ReadData<T>() where T : new();
        bool TryReadEncryptedData(string password);
        void UpdateData<T>(T data, string password);
        void UpdateData<T>(T data);
        bool ValidateData();
        void AddCacheDeleteFlag();
        Task<T> ReadDataAsync<T>(string password) where T : new();
        Task<T> ReadDataAsync<T>() where T : new();
        Task UpdateDataAsync<T>(T data, string password);
        Task UpdateDataAsync<T>(T data);
    }
}