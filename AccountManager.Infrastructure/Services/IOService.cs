﻿using AccountManager.Core.Interfaces;
using AccountManager.Core.Static;
using AccountManager.Core.ViewModels;
using System.Text.Json;

namespace AccountManager.Infrastructure.Services
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
            if (!Directory.Exists(_dataPath))
            {
                Directory.CreateDirectory(_dataPath);
            }
            if (!File.Exists($"{_dataPath}\\data.dat"))
                return false;
            else
                return true;
        }

        public bool TryLogin(string password)
        {
            try
            {
                ReadData<List<AccountListItemViewModel>>(password);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public void UpdateData<T>(T data, string password)
        {
            var fileName = StringEncryption.Hash(typeof(T).Name);
            File.WriteAllText($"{_dataPath}\\{fileName}.dat", StringEncryption.EncryptString(password, JsonSerializer.Serialize(data)));
        }
        public void UpdateData<T>(T data)
        {
            var fileName = StringEncryption.Hash(typeof(T).Name);
            File.WriteAllText($"{_dataPath}\\{fileName}.dat", JsonSerializer.Serialize(data));
        }

        public T ReadData<T>(string password) where T : new()
        {
            var fileName = StringEncryption.Hash(typeof(T).Name);
            if (!File.Exists($"{_dataPath}\\{fileName}.dat"))
            {
                File.WriteAllText($"{_dataPath}\\{fileName}.dat", StringEncryption.EncryptString(password, JsonSerializer.Serialize(new T())));
                return new T();
            }

            string encryptedData = File.ReadAllText($"{_dataPath}\\{fileName}.dat");
            string decryptedData = StringEncryption.DecryptString(password, encryptedData);
            return JsonSerializer.Deserialize<T>(decryptedData);
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
            return JsonSerializer.Deserialize<T>(data);
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
            DriveInfo steamDrive = null;
            foreach (DriveInfo drive in DriveInfo.GetDrives())
            {
                if (Directory.Exists($"{drive.RootDirectory}\\Program Files (x86)\\Steam"))
                {
                    steamDrive = drive;
                }
            }
            return steamDrive;
        }
        private void WriteFile(string filePath, string fileContents)
        {
            File.WriteAllText(filePath, fileContents);
        }

        private string ReadFile(string filePath)
        {
            return File.ReadAllText(filePath);
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

