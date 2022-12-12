using AccountManager.Core.Interfaces;
using AccountManager.Core.Static;
using LazyCache;
using Microsoft.Extensions.Caching.Memory;

namespace AccountManager.Infrastructure.Services.FileSystem
{
    public sealed class CachedGeneralFileSystemService : IGeneralFileSystemService
    {
        public static string DataPath { get; set; } = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "Multi-Account-Manager");
        private readonly IAppCache _memoryCache;
        private readonly GeneralFileSystemService _generalFileSystemService;
        public CachedGeneralFileSystemService(IAppCache memoryCache, GeneralFileSystemService generalFileSystemService)
        {
            _memoryCache = memoryCache;
            _generalFileSystemService = generalFileSystemService;
        }

        public bool ValidateData()
        {
            return _generalFileSystemService.ValidateData();
        }

        public bool TryReadEncryptedData(string password)
        {
            return _generalFileSystemService.TryReadEncryptedData(password);
        }
       
        public bool IsFileLocked(string filePath)
        {
            var cacheKey = $"{filePath}.{nameof(IsFileLocked)}";
            return _memoryCache.GetOrAdd(cacheKey, (entry) =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(1);
                return _generalFileSystemService.IsFileLocked(filePath);
            });
        }

        public void WriteData<T>(T data, string password)
        {
            var name = typeof(T).Name;
            var fileName = StringEncryption.Hash(name);
            fileName = string.Join("_", fileName.Split(Path.GetInvalidFileNameChars()));
            var filePath = Path.Combine(DataPath, $"{fileName}.dat");
            var cacheKey = $"{filePath}.FileData";
            _memoryCache.Remove(cacheKey);

            _generalFileSystemService.WriteData(data, password);
        }

        public void WriteData<T>(T data)
        {
            var type = typeof(T);
            var name = type.Name;
            name += string.Join("-", type.GetGenericArguments().Select(x => x.Name));

            var fileName = StringEncryption.Hash(name);
            fileName = string.Join("_", fileName.Split(Path.GetInvalidFileNameChars()));
            var filePath = Path.Combine(DataPath, $"{fileName}.dat");
            var cacheKey = $"{filePath}.FileData";
            _memoryCache.Remove(cacheKey);

            _generalFileSystemService.WriteData(data);
        }

        public async Task WriteDataAsync<T>(T data)
        {
            var type = typeof(T);
            var name = type.Name;
            name += string.Join("-", type.GetGenericArguments().Select(x => x.Name));

            var fileName = StringEncryption.Hash(name);
            fileName = string.Join("_", fileName.Split(Path.GetInvalidFileNameChars()));
            var filePath = Path.Combine(DataPath, $"{fileName}.dat");
            var cacheKey = $"{filePath}.FileData";
            _memoryCache.Remove(cacheKey);

            await _generalFileSystemService.WriteDataAsync(data);
        }

        public async Task WriteDataAsync<T>(T data, string password)
        {
            var name = typeof(T).Name;
            var fileName = StringEncryption.Hash(name);
            fileName = string.Join("_", fileName.Split(Path.GetInvalidFileNameChars()));
            var filePath = Path.Combine(DataPath, $"{fileName}.dat");
            var cacheKey = $"{filePath}.FileData";
            _memoryCache.Remove(cacheKey);
            
            await _generalFileSystemService.WriteDataAsync(data, password);
        }

        public T ReadData<T>(string password) where T : new()
        {
            var name = typeof(T).Name;
            var fileName = StringEncryption.Hash(name);
            fileName = string.Join("_", fileName.Split(Path.GetInvalidFileNameChars()));
            var filePath = Path.Combine(DataPath, $"{fileName}.dat");
            var cacheKey = $"{filePath}.FileData";

            return _memoryCache.GetOrAdd(cacheKey, (entry) =>
            {
                return _generalFileSystemService.ReadData<T>(password);
            }) ?? new T();
        }

        public T ReadData<T>() where T : new()
        {
            var type = typeof(T);
            var name = type.Name;
            name += string.Join("-", type.GetGenericArguments().Select(x => x.Name));

            var fileName = StringEncryption.Hash(name);
            fileName = string.Join("_", fileName.Split(Path.GetInvalidFileNameChars()));
            var filePath = Path.Combine(DataPath, $"{fileName}.dat");
            var cacheKey = $"{filePath}.FileData";

            return _memoryCache.GetOrAdd(cacheKey, (entry) =>
            {
                return  _generalFileSystemService.ReadData<T>();
            }) ?? new T();
        }

        public async Task<T> ReadDataAsync<T>() where T : new()
        {
            var type = typeof(T);
            var name = type.Name;
            name += string.Join("-", type.GetGenericArguments().Select(x => x.Name));

            var fileName = StringEncryption.Hash(name);
            fileName = string.Join("_", fileName.Split(Path.GetInvalidFileNameChars()));
            var filePath = Path.Combine(DataPath, $"{fileName}.dat");
            var cacheKey = $"{filePath}.FileData";

            return (await _memoryCache.GetOrAddAsync(cacheKey, async (entry) =>
            {
                return await _generalFileSystemService.ReadDataAsync<T>();
            })) ?? new T();
        }

        public async Task<T> ReadDataAsync<T>(string password) where T : new()
        {
            var name = typeof(T).Name;
            var fileName = StringEncryption.Hash(name);
            fileName = string.Join("_", fileName.Split(Path.GetInvalidFileNameChars()));
            var filePath = Path.Combine(DataPath, $"{fileName}.dat");
            var cacheKey = $"{filePath}.FileData";

            return (await _memoryCache.GetOrAddAsync(cacheKey, async (entry) =>
            {
                return await _generalFileSystemService.ReadDataAsync<T>(password);
            })) ?? new T();
        }

        public void AddCacheDeleteFlag()
        {
            _generalFileSystemService.AddCacheDeleteFlag();
        }

        public async Task<T> ReadDataAsync<T>(string filePath, string password) where T : new()
        {
            var cacheKey = $"FileContents.{filePath}";
            return (await _memoryCache.GetOrAddAsync(cacheKey, async (entry) =>
            {
                return await _generalFileSystemService.ReadDataAsync<T>(password);
            })) ?? new T();
        }

        public async Task WriteUnmanagedData<T>(T data, string filePath, string password)
        {
            await _generalFileSystemService.WriteUnmanagedData<T>(data, filePath, password);
        }

        public async Task<T> ReadUnmanagedData<T>(string filePath, string password) where T : new()
        {
            var cacheKey = $"FileContentsUnmanaged.{filePath}";
            return (await _memoryCache.GetOrAddAsync(cacheKey, async (entry) =>
            {
                return await _generalFileSystemService.ReadUnmanagedData<T>(filePath, password);
            })) ?? new T();
        }
    }
}