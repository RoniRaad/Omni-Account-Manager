using AccountManager.Core.Interfaces;
using AccountManager.Core.Models;
using AccountManager.Core.Models.UserSettings;
using AccountManager.Core.Services;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;

namespace AccountManager.Infrastructure.Services.Platform
{
    public class EpicGamesPlatformService : IPlatformService
    {
        private static string EncryptionKey = "A09C853C9E95409BB94D707EADEFA52E";
        private readonly IAlertService _alertService;
        private readonly ILogger<EpicGamesPlatformService> _logger;
        private readonly IMemoryCache _memoryCache;
        private readonly IDistributedCache _persistantCache;
        private readonly IEpicGamesLibraryService _epicGamesLibraryService;
        private readonly IUserSettingsService<GeneralSettings> _settingsService;
        private readonly IEpicGamesExternalAuthService _epicGamesExternalAuthService;
        private readonly AppState _state;
        public static string WebIconFilePath = Path.Combine("logos", "epic-games-logo.png");
        public static string IcoFilePath = Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly()?.Location)
            ?? ".", "ShortcutIcons", "epic-logo.ico");
        public EpicGamesPlatformService( IAlertService alertService,
            IMemoryCache memoryCache, IUserSettingsService<GeneralSettings> settingsService,
            ILogger<EpicGamesPlatformService> logger, IEpicGamesExternalAuthService epicGamesExternalAuthService,
            IDistributedCache persistantCache, IEpicGamesLibraryService epicGamesLibraryService, AppState state)
        {
            _alertService = alertService;
            _memoryCache = memoryCache;
            _settingsService = settingsService;
            _logger = logger;
            _epicGamesExternalAuthService = epicGamesExternalAuthService;
            _persistantCache = persistantCache;
            _epicGamesLibraryService = epicGamesLibraryService;
            _state = state;
        }

        public async Task Login(Account account)
        {
            var tokens = await _epicGamesExternalAuthService.TryGetEpicGamesAccessTokens(account.Username, account.Password);
            if (tokens?.RefreshToken is null)
            {
                _alertService.AddErrorAlert("There was an error attempting to sign in.");
                _logger.LogError("Epic games login failed! No tokens were returned!");
                return;
            }

            account.PlatformId = tokens.Id;

            CloseEpicGamesClient();
            await SetEpicGamesTokenFile(tokens.Username ?? "", tokens.Name ?? "", tokens.LastName ?? "", tokens.DisplayName ?? "", tokens.RefreshToken);
            var gameId = await _persistantCache.GetStringAsync($"{account.Guid}.SelectedEpicGame");
            if (!string.IsNullOrEmpty(gameId) && gameId != "none")
            {
                if (!TryLaunchEpicGamesGame(gameId))
                {
                    _alertService.AddErrorAlert("There was an error attempting to start the selected game.");
                    _logger.LogError("There was an error attempting to start the selected game!");
                }
                else
                    return;
            }

            LaunchEpicGamesClient();
        }

        public async Task<(bool, Rank)> TryFetchRank(Account account)
        {
            var rankCacheString = $"{account.Username}.epicgames.rank";
            if (_memoryCache.TryGetValue(rankCacheString, out Rank? rank) && rank is not null)
                return (true, rank);

            rank = new Rank();
            try
            {
               // if (string.IsNullOrEmpty(account.PlatformId))
                   // account.PlatformId = await _riotClient.GetPuuId(account);
                if (string.IsNullOrEmpty(account.PlatformId))
                    return (false, rank);

               // rank = await _valorantClient.GetValorantRank(account);

                if (!string.IsNullOrEmpty(rank?.Tier))
                    _memoryCache.Set(rankCacheString, rank, TimeSpan.FromHours(1));

                if (rank is null)
                    return (false, new Rank());

                return new(true, rank);
            }
            catch
            {
                return new(false, new Rank());
            }
        }

        public async Task<(bool, string)> TryFetchId(Account account)
        {
            try
            {
                if (!string.IsNullOrEmpty(account.PlatformId))
                {
                    return new (true, account.PlatformId);
                }

                //var id = await _riotClient.GetPuuId(account);
                return new(false, string.Empty);
            }
            catch
            {
                return new (false, string.Empty);
            }
        }


        private async Task SetEpicGamesTokenFile(string email, string fName, string lName, string dName, string token)
        {
            var launcherTokenString = Encrypt(GenerateLoginJson(email, fName, lName, dName, token), EncryptionKey);
            var localAppDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            var path = System.IO.Path.Combine(localAppDataPath, "EpicGamesLauncher", "Saved", "Config", "Windows", "GameUserSettings.ini");
            if (File.Exists(path))
            {
                var lines = File.ReadAllLines(path);
                for (int i = 0; i < lines.Length; i++)
                {
                    if (lines[i] == "[RememberMe]")
                    {
                        lines[i + 1] = $"Data={launcherTokenString}";
                        lines[i + 2] = "Enable=True";
                    }

                }

                await File.WriteAllLinesAsync(path, lines);
            }
        }

        private void CloseEpicGamesClient()
        {
            foreach (var process in Process.GetProcesses())
                if (process.ProcessName.Contains("EpicGamesLauncher"))
                    process.Kill();
        }

        private void LaunchEpicGamesClient()
        {
            Process.Start("C:\\Program Files (x86)\\Epic Games\\Launcher\\Portal\\Binaries\\Win32\\EpicGamesLauncher.exe");
        }

        private bool TryLaunchEpicGamesGame(string appId)
        {
            if (!_epicGamesLibraryService.TryGetInstalledGames(out var installedGames))
                return false;

            var game = installedGames.FirstOrDefault((game) => game.AppId == appId);
            if (game?.InstallLocation is not null && game?.LaunchExecutable is not null)
            {
                var processStart = new ProcessStartInfo();
                processStart.WorkingDirectory = game.InstallLocation;
                processStart.FileName = Path.Combine(game.InstallLocation, game.LaunchExecutable);
                Process.Start(processStart);

            }
            else
                return false;

            return true;
        }

        static string Decrypt(string toDecrypt, string key)
        {
            byte[] keyArray = UTF8Encoding.UTF8.GetBytes(key); // AES-256 key
            PadToMultipleOf(ref keyArray, 8);
            byte[] toEncryptArray = Convert.FromBase64String(toDecrypt);
            //byte[] toEncryptArray = ConvertHexStringToByteArray(toDecrypt);

            Aes rDel = Aes.Create();
            rDel.KeySize = (keyArray.Length * 8);
            rDel.Key = keyArray;          // in bits
            rDel.Mode = CipherMode.ECB; // http://msdn.microsoft.com/en-us/library/system.security.cryptography.ciphermode.aspx
            rDel.Padding = PaddingMode.PKCS7;  // better lang support
            ICryptoTransform cTransform = rDel.CreateDecryptor();
            byte[] resultArray = cTransform.TransformFinalBlock(toEncryptArray, 0, toEncryptArray.Length);
            return UTF8Encoding.UTF8.GetString(resultArray);
        }

        static string Encrypt(string toEncrypt, string key)
        {
            byte[] keyArray = UTF8Encoding.UTF8.GetBytes(key); // AES-256 key
            PadToMultipleOf(ref keyArray, 8);
            byte[] toEncryptArray = UTF8Encoding.UTF8.GetBytes(toEncrypt);
            //byte[] toEncryptArray = ConvertHexStringToByteArray(toDecrypt);

            Aes rDel = Aes.Create();
            rDel.KeySize = (keyArray.Length * 8);
            rDel.Key = keyArray;          // in bits
            rDel.Mode = CipherMode.ECB; // http://msdn.microsoft.com/en-us/library/system.security.cryptography.ciphermode.aspx
            rDel.Padding = PaddingMode.PKCS7;  // better lang support
            ICryptoTransform cTransform = rDel.CreateEncryptor();
            byte[] resultArray = cTransform.TransformFinalBlock(toEncryptArray, 0, toEncryptArray.Length);
            return Convert.ToBase64String(resultArray);
        }

        static void PadToMultipleOf(ref byte[] src, int pad)
        {
            int len = (src.Length + pad - 1) / pad * pad;
            Array.Resize(ref src, len);
        }

        public string GenerateLoginJson(string email, string fName, string lName, string dName, string token)
        {
            return $"[{{\"Region\":\"Prod\",\"Email\":\"{email}\",\"Name\":\"{fName}\",\"LastName\":\"{lName}\",\"DisplayName\":\"{dName}\",\"Token\":\"{token}\",\"bHasPasswordAuth\":true}}]\0";
        }
    }
}
