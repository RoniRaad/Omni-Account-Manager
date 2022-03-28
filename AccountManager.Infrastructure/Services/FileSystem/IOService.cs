using AccountManager.Core.Interfaces;
using AccountManager.Core.Models;
using AccountManager.Core.Static;
using System.Text.Json;

namespace AccountManager.Infrastructure.Services.FileSystem
{
    public class IOService : IIOService
    {
        public IOService()
        {
            ValidateData();
        }

        private string _dataPath { get; set; } = $"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}\\Multi-Account-Manager";
        public bool ValidateData()
        {
            var fileName = StringEncryption.Hash(typeof(List<Account>).Name);
            if (!Directory.Exists(_dataPath))
            {
                Directory.CreateDirectory(_dataPath);
            }
            if (!File.Exists($"{_dataPath}\\{fileName}.dat"))
                return false;
            else
                return true;
        }

        public bool TryLogin(string password)
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
                using (FileStream inputStream = File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.None))
                {
                    return inputStream.Length <= 0;
                }
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
            var serializedData = JsonSerializer.Serialize(data);
            var encryptedData = StringEncryption.EncryptString(password, serializedData);
            File.WriteAllText($"{_dataPath}\\{fileName}.dat", encryptedData);
        }
        public void UpdateData<T>(T data)
        {
            var fileName = StringEncryption.Hash(typeof(T).Name);
            File.WriteAllText($"{_dataPath}\\{fileName}.dat", JsonSerializer.Serialize(data));
        }

        public T ReadData<T>(string password) where T : new()
        {
            var name = typeof(T).Name;
            var fileName = StringEncryption.Hash(name);
            if (!File.Exists($"{_dataPath}\\{fileName}.dat"))
            {
                File.WriteAllText($"{_dataPath}\\{fileName}.dat", StringEncryption.EncryptString(password, JsonSerializer.Serialize(new T())));
                return new T();
            }

            string encryptedData = File.ReadAllText($"{_dataPath}\\{fileName}.dat");
            string decryptedData = StringEncryption.DecryptString(password, encryptedData);
            return JsonSerializer.Deserialize<T>(decryptedData) ?? new T();
        }
        public T ReadData<T>() where T : new()
        {
            var fileName = StringEncryption.Hash(typeof(T).Name);
            if (!File.Exists($"{_dataPath}\\{fileName}.dat"))
            {
                File.WriteAllText($"{_dataPath}\\{fileName}.dat", JsonSerializer.Serialize(new T()));
                return new T();
            }

            string data = File.ReadAllText($"{_dataPath}\\{fileName}.dat");
            return JsonSerializer.Deserialize<T>(data) ?? new T();
        }

        public string GetEncryptedUsername()
        {
            try
            {
                return File.ReadAllText($"{_dataPath}\\username.txt");
            }
            catch
            {
                return "";
            }
        }

        public DriveInfo FindSteamDrive()
        {
            var drives = DriveInfo.GetDrives();
            return drives
                .Where((drive) => Directory.Exists($"{drive.RootDirectory}\\Program Files (x86)\\Steam"))
                .FirstOrDefault(drives.First());
        }

        public List<string[]> GetInstalledGamesManifest()
        {
            string[] steamAppFiles = Directory.GetFiles($"{FindSteamDrive()}\\Program Files (x86)\\Steam\\steamapps");
            List<string[]> steamGames = new List<string[]>();

            foreach (string file in steamAppFiles)
                if (file.Contains("appmanifest"))
                {
                    string[] fileContents = File.ReadAllLines(file);
                    steamGames.Add(fileContents);
                }

            return steamGames;
        }
    }
}

