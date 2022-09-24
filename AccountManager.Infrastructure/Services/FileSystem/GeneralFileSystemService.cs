using AccountManager.Core.Interfaces;
using AccountManager.Core.Models;
using AccountManager.Core.Static;
using Microsoft.Extensions.Caching.Memory;
using System.Text.Json;

namespace AccountManager.Infrastructure.Services.FileSystem
{
    public sealed class GeneralFileSystemService : IGeneralFileSystemService
    {
        public static string DataPath { get; set; } = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "Multi-Account-Manager");
        private readonly IMemoryCache _memoryCache;
        public GeneralFileSystemService(IMemoryCache memoryCache)
        {
            _memoryCache = memoryCache;
        }

        public static void InitializeFileSystem()
        {
            // Initialize datapath
            if (!Directory.Exists(DataPath))
            {
                Directory.CreateDirectory(DataPath);
            }
            var deleteCacheFlagFilePath = Path.Combine(DataPath, "deletecache");
            var cacheDatabasePath = Path.Combine(DataPath, "cache.db");

            // This file acts as a flag to delete the cache file before initializing
            if (File.Exists(deleteCacheFlagFilePath))
            {
                File.Delete(cacheDatabasePath);
                File.Delete(deleteCacheFlagFilePath);
            }
        }

        public bool ValidateData()
        {
            var fileName = StringEncryption.Hash(typeof(List<Account>).Name);
            fileName = string.Join("_", fileName.Split(Path.GetInvalidFileNameChars()));
            var filePath = Path.Combine(DataPath, $"{fileName}.dat");
            AddAccountDataToCache();

            if (!File.Exists(filePath))
                return false;
            else
                return true;

        }

        private void AddAccountDataToCache()
        {
            var name = typeof(List<Account>).Name;
            var fileName = StringEncryption.Hash(name);
            fileName = string.Join("_", fileName.Split(Path.GetInvalidFileNameChars()));
            var filePath = Path.Combine(DataPath, $"{fileName}.dat");
            ReadFile(filePath);
        }

        public bool TryReadEncryptedData(string password)
        {
            try
            {
                ReadData<List<Account>>(password);
                return true;
            }
            catch
            {
                return false;
            }
        }
       
        public bool IsFileLocked(string filePath)
        {

            if (!File.Exists(filePath))
                return false;

            try
            {
                using FileStream inputStream = File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.None);
                return inputStream.Length <= 0;
            }
            catch (Exception)
            {
                return true;
            }

        }

        public void UpdateData<T>(T data, string password)
        {
            var name = typeof(T).Name;
            var fileName = StringEncryption.Hash(name);
            fileName = string.Join("_", fileName.Split(Path.GetInvalidFileNameChars()));
            var filePath = Path.Combine(DataPath, $"{fileName}.dat");

            var serializedData = JsonSerializer.Serialize(data);
            var encryptedData = StringEncryption.EncryptString(password, serializedData);
            WriteFile(filePath, encryptedData);
        }

        public void UpdateData<T>(T data)
        {
            var type = typeof(T);
            var name = type.Name;
            name += string.Join("-", type.GetGenericArguments().Select(x => x.Name));

            var fileName = StringEncryption.Hash(name);
            fileName = string.Join("_", fileName.Split(Path.GetInvalidFileNameChars()));
            var filePath = Path.Combine(DataPath, $"{fileName}.dat");

            WriteFile(filePath, JsonSerializer.Serialize(data));
        }

        public async Task UpdateDataAsync<T>(T data)
        {
            var type = typeof(T);
            var name = type.Name;
            name += string.Join("-", type.GetGenericArguments().Select(x => x.Name));

            var fileName = StringEncryption.Hash(name);
            fileName = string.Join("_", fileName.Split(Path.GetInvalidFileNameChars()));
            var filePath = Path.Combine(DataPath, $"{fileName}.dat");

            await WriteFileAsync(filePath, JsonSerializer.Serialize(data));
        }

        public async Task UpdateDataAsync<T>(T data, string password)
        {
            var name = typeof(T).Name;
            var fileName = StringEncryption.Hash(name);
            fileName = string.Join("_", fileName.Split(Path.GetInvalidFileNameChars()));
            var filePath = Path.Combine(DataPath, $"{fileName}.dat");

            var serializedData = JsonSerializer.Serialize(data);
            var encryptedData = StringEncryption.EncryptString(password, serializedData);
            await WriteFileAsync(filePath, encryptedData);
        }

        public T ReadData<T>(string password) where T : new()
        {
            string decryptedData;
            var name = typeof(T).Name;
            var fileName = StringEncryption.Hash(name);
            fileName = string.Join("_", fileName.Split(Path.GetInvalidFileNameChars()));
            var filePath = Path.Combine(DataPath, $"{fileName}.dat");

            if (!File.Exists(filePath))
            {
                WriteFile(filePath, StringEncryption.EncryptString(password, JsonSerializer.Serialize(new T())));
                return new T();
            }

            string encryptedData = ReadFile(filePath);
            try
            {
                decryptedData = StringEncryption.DecryptString(password, encryptedData);
            }
            catch
            {
                throw new ArgumentException("Incorrect Password Given");
            }
            return JsonSerializer.Deserialize<T>(decryptedData) ?? new T();
        }

        public T ReadData<T>() where T : new()
        {
            var type = typeof(T);
            var name = type.Name;
            name += string.Join("-", type.GetGenericArguments().Select(x => x.Name));

            var fileName = StringEncryption.Hash(name);
            fileName = string.Join("_", fileName.Split(Path.GetInvalidFileNameChars()));
            var filePath = Path.Combine(DataPath, $"{fileName}.dat");

            if (!File.Exists(filePath))
            {
                WriteFile(filePath, JsonSerializer.Serialize(new T()));
                return new T();
            }

            string data = ReadFile(filePath);
            return JsonSerializer.Deserialize<T>(data) ?? new T();
        }

        public async Task<T> ReadDataAsync<T>() where T : new()
        {
            var type = typeof(T);
            var name = type.Name;
            name += string.Join("-", type.GetGenericArguments().Select(x => x.Name));

            var fileName = StringEncryption.Hash(name);
            fileName = string.Join("_", fileName.Split(Path.GetInvalidFileNameChars()));
            var filePath = Path.Combine(DataPath, $"{fileName}.dat");

            if (!File.Exists(filePath))
            {
                await WriteFileAsync(filePath, JsonSerializer.Serialize(new T()));
                return new T();
            }

            string data = await ReadFileAsync(filePath);
            return JsonSerializer.Deserialize<T>(data) ?? new T();
        }

        public async Task<T> ReadDataAsync<T>(string password) where T : new()
        {
            string decryptedData;
            var name = typeof(T).Name;
            var fileName = StringEncryption.Hash(name);
            fileName = string.Join("_", fileName.Split(Path.GetInvalidFileNameChars()));
            var filePath = Path.Combine(DataPath, $"{fileName}.dat");

            if (!File.Exists(filePath))
            {
                await WriteFileAsync(filePath, StringEncryption.EncryptString(password, JsonSerializer.Serialize(new T())));
                return new T();
            }

            string encryptedData = await ReadFileAsync(filePath);
            try
            {
                decryptedData = StringEncryption.DecryptString(password, encryptedData);
            }
            catch
            {
                throw new ArgumentException("Incorrect Password Given");
            }
            return JsonSerializer.Deserialize<T>(decryptedData) ?? new T();
        }

        public void AddCacheDeleteFlag()
        {
            var filePath = Path.Combine(DataPath, "deletecache");

            File.Create(filePath);
        }

        private string ReadFile(string filePath)
        {
            if (!File.Exists(filePath))
                return "";

            var cacheKey = $"{filePath}.FileContent";

            return _memoryCache.GetOrCreate(cacheKey, (entry) =>
            {
                return File.ReadAllText(filePath);
            }) ?? "";
        }

        private async Task<string> ReadFileAsync(string filePath)
        {
            if (!File.Exists(filePath))
                return "";

            var cacheKey = $"{filePath}.FileContent";
            return await _memoryCache.GetOrCreateAsync(cacheKey, async (entry) =>
            {
                return await File.ReadAllTextAsync(filePath);
            }) ?? "";
        }

        private void WriteFile(string filePath, string content)
        {
            var dir = Path.GetDirectoryName(filePath);
            if (dir is null)
                return;

            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            var cacheKey = $"{filePath}.FileContent";
            _memoryCache.Remove(cacheKey);

            File.WriteAllText(filePath, content);
        }

        private async Task WriteFileAsync(string filePath, string content)
        {
            var dir = Path.GetDirectoryName(filePath);
            var cacheKey = $"{filePath}.FileContent";
            _memoryCache.Remove(cacheKey);

            if (dir is null)
                return;

            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            await File.WriteAllTextAsync(filePath, content);
        }
    }
}