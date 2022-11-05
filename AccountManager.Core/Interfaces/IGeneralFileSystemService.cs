namespace AccountManager.Core.Interfaces
{
    public interface IGeneralFileSystemService
    {
        bool IsFileLocked(string filePath);
        T ReadData<T>(string password) where T : new();
        T ReadData<T>() where T : new();
        bool TryReadEncryptedData(string password);
        void WriteData<T>(T data, string password);
        void WriteData<T>(T data);
        bool ValidateData();
        void AddCacheDeleteFlag();
        Task<T> ReadDataAsync<T>(string password) where T : new();
        Task<T> ReadDataAsync<T>() where T : new();
        Task WriteDataAsync<T>(T data, string password);
        Task WriteDataAsync<T>(T data);
        Task<T> ReadDataAsync<T>(string filePath, string password) where T : new();
        Task WriteUnmanagedData<T>(T data, string filePath, string password);
        Task<T> ReadUnmanagedData<T>(string filePath, string password) where T : new();
    }
}