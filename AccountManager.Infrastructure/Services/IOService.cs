using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AccountManager.Infrastructure.Services
{
    public class IOService
    {
        private string _dataPath { get; set; } = $"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}\\Account Manager";
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

        public void UpdateData(string data, string password)
        {
            File.WriteAllText($"{_dataPath}\\data.dat", StringEncryption.EncryptString(password, data));
        }

        public string ReadData(string password)
        {
            if (!File.Exists($"{_dataPath}\\data.dat"))
            {
                InitializeData(password);
                return "[]";

            }

            string encryptedData = File.ReadAllText($"{_dataPath}\\data.dat");
            string decryptedData = StringEncryption.DecryptString(password, encryptedData);
            return StringEncryption.DecryptString(password, encryptedData);
        }
        public void InitializeData(string password)
        {
            File.WriteAllText($"{_dataPath}\\data.dat", StringEncryption.EncryptString(password, "[]"));
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

        public void SaveConfig(string contents)
        {
            WriteFile($"{_dataPath}\\config.conf", contents);
        }

        public string GetConfig()
        {
            try
            {
                return ReadFile($"{_dataPath}\\config.conf");
            }
            catch
            {
                return "null";
            }
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

