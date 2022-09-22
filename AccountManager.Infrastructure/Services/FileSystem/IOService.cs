﻿using AccountManager.Core.Interfaces;
using AccountManager.Core.Models;
using AccountManager.Core.Static;
using Microsoft.Extensions.Caching.Memory;
using System.Text.Json;

namespace AccountManager.Infrastructure.Services.FileSystem
{
    public sealed class IOService : IIOService
    {
        public static string DataPath { get; set; } = $"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}\\Multi-Account-Manager";
        private readonly IMemoryCache _memoryCache;
        public IOService(IMemoryCache memoryCache)
        {
            ValidateData();
            _memoryCache = memoryCache;
        }

        public bool ValidateData()
        {
            var fileName = StringEncryption.Hash(typeof(List<Account>).Name);
            fileName = string.Join("_", fileName.Split(Path.GetInvalidFileNameChars()));

            if (!Directory.Exists(DataPath))
            {
                Directory.CreateDirectory(DataPath);
            }
            if (!File.Exists($"{DataPath}\\{fileName}.dat"))
                return false;
            else
                return true;
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
            var cacheKey = $"{filePath}.{nameof(IsFileLocked)}";
            return _memoryCache.GetOrCreate(cacheKey, (entry) =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(15);
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
            });
        }

        public void UpdateData<T>(T data, string password)
        {
            var name = typeof(T).Name;
            var fileName = StringEncryption.Hash(name);
            fileName = string.Join("_", fileName.Split(Path.GetInvalidFileNameChars()));

            var serializedData = JsonSerializer.Serialize(data);
            var encryptedData = StringEncryption.EncryptString(password, serializedData);
            File.WriteAllText($"{DataPath}\\{fileName}.dat", encryptedData);
        }

        public void UpdateData<T>(T data)
        {
            var type = typeof(T);
            var name = type.Name;
            name += string.Join("-", type.GetGenericArguments().Select(x => x.Name));

            var fileName = StringEncryption.Hash(name);
            fileName = string.Join("_", fileName.Split(Path.GetInvalidFileNameChars()));

            File.WriteAllText($"{DataPath}\\{fileName}.dat", JsonSerializer.Serialize(data));
        }

        public T ReadData<T>(string password) where T : new()
        {
            string decryptedData;
            var name = typeof(T).Name;
            var fileName = StringEncryption.Hash(name);
            fileName = string.Join("_", fileName.Split(Path.GetInvalidFileNameChars()));

            if (!File.Exists($"{DataPath}\\{fileName}.dat"))
            {
                File.WriteAllText($"{DataPath}\\{fileName}.dat", StringEncryption.EncryptString(password, JsonSerializer.Serialize(new T())));
                return new T();
            }

            string encryptedData = File.ReadAllText($"{DataPath}\\{fileName}.dat");
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

            if (!File.Exists($"{DataPath}\\{fileName}.dat"))
            {
                File.WriteAllText($"{DataPath}\\{fileName}.dat", JsonSerializer.Serialize(new T()));
                return new T();
            }

            string data = File.ReadAllText($"{DataPath}\\{fileName}.dat");
            return JsonSerializer.Deserialize<T>(data) ?? new T();
        }

        public void AddCacheDeleteFlag()
        {
            File.Create(@$"{IOService.DataPath}\deletecache");
        }
    }
}