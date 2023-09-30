using AccountManager.Core.Interfaces;
using AccountManager.Core.Models;
using System.Diagnostics;
using System.Net.Http.Json;
using AccountManager.Core.Enums;
using AccountManager.Infrastructure.Services.FileSystem;
using AccountManager.Core.Models.RiotGames.Requests;
using Microsoft.Extensions.Caching.Memory;
using AccountManager.Core.Exceptions;
using System.Reflection;
using AccountManager.Core.Models.UserSettings;
using AccountManager.Infrastructure.Clients;
using Microsoft.Extensions.Logging;
using AccountManager.Core.Models.RiotGames;
using Microsoft.Extensions.Caching.Distributed;
using System.IdentityModel.Tokens.Jwt;
using System.Text.Json;
using AccountManager.Core.Static;

namespace AccountManager.Infrastructure.Services.Platform
{
    public sealed class LeaguePlatformService : IPlatformService
    {
        private readonly ITokenService _riotService;
        private readonly ILeagueClient _leagueClient;
        private readonly IRiotClient _riotClient;
        private readonly HttpClient _httpClient;
        private readonly IAlertService _alertService;
        private readonly ILogger<LeaguePlatformService> _logger;
        private readonly IDistributedCache _persistantCache;
        private readonly IRiotFileSystemService _riotFileSystemService;
        private readonly IUserSettingsService<GeneralSettings> _settingsService;
        private readonly IRiotTokenClient _riotTokenClient;
        public static string WebIconFilePath = Path.Combine("logos", "league-logo.png");
        public static string IcoFilePath = Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly()?.Location)
            ?? ".", "ShortcutIcons", "league-logo.ico");
        public LeaguePlatformService(ILeagueClient leagueClient, IRiotClient riotClient, IGenericFactory<AccountType,
            ITokenService> tokenServiceFactory, IHttpClientFactory httpClientFactory, IRiotFileSystemService riotFileSystemService,
            IAlertService alertService, IDistributedCache persistantCache, IUserSettingsService<GeneralSettings> settingsService,
            IRiotTokenClient riotTokenClient, ILogger<LeaguePlatformService> logger)
        {
            _leagueClient = leagueClient;
            _riotClient = riotClient;
            _riotService = tokenServiceFactory.CreateImplementation(AccountType.Valorant);
            _httpClient = httpClientFactory.CreateClient("SSLBypass");
            _riotFileSystemService = riotFileSystemService;
            _alertService = alertService;
            _persistantCache = persistantCache;
            _settingsService = settingsService;
            _riotTokenClient = riotTokenClient;
            _logger = logger;
        }

        public async Task Login(Account account)
        {
            if (await TryLoginUsingApi(account))
                return;
            if (await TryLoginUsingRCU(account))
                return;

            _alertService.AddErrorAlert("There was an error attempting to sign in.");
            _logger.LogError("Riot login failed via api and rcu!");
        }

        public async Task<(bool, Rank)> TryFetchRank(Account account)
        {
            var rankCacheString = $"{account.Username}.leagueoflegends.rank";
            var rank = await _persistantCache.GetAsync<Rank>(rankCacheString);
            if ( rank is not null)
                return (true, rank);
            
            rank = new Rank();
            try
            {
                if (string.IsNullOrEmpty(account.PlatformId))
                    account.PlatformId = await _riotClient.GetPuuId(account);
                if (string.IsNullOrEmpty(account.PlatformId))
                    return (false, rank);

                rank = await _leagueClient.GetSummonerRankByPuuidAsync(account);

                if (!string.IsNullOrEmpty(rank?.Tier))
                    await _persistantCache.SetAsync(rankCacheString, rank, TimeSpan.FromHours(1));

                if (rank is null)
                    return (false, new Rank());

                return (true, rank);
            }
            catch
            {
                return (false, rank);
            }
        }

        public async Task<(bool, string)> TryFetchId(Account account)
        {
            try
            {
                if (!string.IsNullOrEmpty(account.PlatformId))
                    return (true, account.PlatformId);

                var id = await _riotClient.GetPuuId(account);
                return (id is not null, id ?? string.Empty);
            }
            catch
            {
                return (false, string.Empty);
            }
        }

        private async Task<bool> TryLoginUsingRCU(Account account)
        {
            try
            {
                CloseAllRiotApps();

                await _riotFileSystemService.WaitForClientClose();
                _riotFileSystemService.DeleteLockfile();

                var startRiot = new ProcessStartInfo
                {
                    FileName = GetRiotExePath(),
                };

                Process.Start(startRiot);

                await _riotFileSystemService.WaitForClientInit();

                if (!await SendCredentialsToClient(account))
                {
                    return false;
                }

                StartLeague();
                return true;
            }
            catch
            {
                return false;
            }
        }

        private async Task<bool> TryLoginUsingApi(Account account)
        {
            RegionInfo regionInfo;
            try
            {
                _logger.LogInformation("Attempting to login to account! Username: {Username}, Account Type: {AccountType}", account.Username, account.AccountType);

                try
                {
                    regionInfo = await _riotClient.GetValorantRegionInfo(account);
                }
                catch
                {
                    _logger.LogError("Unable to obtain region info for riot account! Account Username: {Username}", account.Username);
                    regionInfo = new();
                }

                CloseAllRiotApps();

                await _riotFileSystemService.WaitForClientClose();
                _riotFileSystemService.DeleteLockfile();
                _logger.LogInformation("Riot client closed and lockfile deleted!");

                var request = new RiotTokenRequest
                {
                    Id = "riot-client",
                    Nonce = "1",
                    RedirectUri = "http://localhost/redirect",
                    ResponseType = "token id_token",
                    Scope = "openid offline_access lol ban profile email phone account"
                };

                _logger.LogInformation("Attempted to retrieve riot tokens!");

                var riotTokens = await _riotTokenClient.GetRiotTokens(request, account);
                if (riotTokens is null || riotTokens?.Cookies?.Tdid is null || riotTokens?.Cookies?.Ssid is null ||
                    riotTokens?.Cookies?.Sub is null || riotTokens?.Cookies?.Csid is null)
                {
                    _logger.LogError("Unable to retrieve riot login cookies! There must have been an issue with the auth request! Account Username: {Username}", account.Username);

                    return false;
                }

                _logger.LogInformation("Riot token obtained successfully!");

                JwtSecurityTokenHandler jwtSecurityTokenHandler = new();
                var jwtToken = jwtSecurityTokenHandler.ReadJwtToken(riotTokens.AccessToken);
                jwtToken.Payload.TryGetValue("dat", out object? regionCodeObject);
                var regionCodeTokenJson = regionCodeObject?.ToString();
                var region = "NA";

                if (regionCodeTokenJson is not null)
                {
                    var accessTokenPayloadData = JsonSerializer.Deserialize<RiotAccessTokenPayloadData>(regionCodeTokenJson);
                    region = accessTokenPayloadData?.Region?.ToUpper()?.TrimEnd('1') ?? "NA";
                }

                await _riotFileSystemService.WriteRiotYaml(region, riotTokens.Cookies.Tdid.Value, riotTokens.Cookies.Ssid.Value,
                    riotTokens.Cookies.Sub.Value, riotTokens.Cookies.Csid.Value);

                StartLeague();

                if (!await VerifyLogInStatus() && !await SendCredentialsToClient(account))
                {
                    return false;
                }

                return true;
            }
            catch (RiotClientNotFoundException)
            {
                _alertService.AddErrorAlert("Could not find riot client. Please set your riot install location in the settings page.");
                _logger.LogError("Could not find riot client! Unable to login!");

                return true;
            }
            catch
            {
                return false;
            }
        }

        private async Task<bool> SendCredentialsToClient(Account account)
        {
            if (!_riotService.TryGetPortAndToken(out var token, out var port))
                return false;

            _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", Convert.ToBase64String(System.Text.Encoding.ASCII.GetBytes($"riot:{token}")));

            await _httpClient.DeleteAsync($"https://127.0.0.1:{port}/rso-auth/v1/session");

            await _httpClient.PostAsJsonAsync($"https://127.0.0.1:{port}/rso-auth/v2/authorizations", new RcuAuthorizationsRequest
            {
                ClientId = "riot-client",
                TrustLevels = new() { "always_trusted" }
            });

            var resp = await _httpClient.PutAsJsonAsync($"https://127.0.0.1:{port}/rso-auth/v1/session/credentials", new RiotClientApi.LoginRequest
            {
                Username = account.Username,
                Password = account.Password,
                PersistLogin = true,
            });

            var credentialsResponse = await resp.Content.ReadFromJsonAsync<RiotClientApi.CredentialsResponse>();

            if (string.IsNullOrEmpty(credentialsResponse?.Type))
            {
                _alertService.AddErrorAlert("There was an error signing in, please try again later.");
                return false;
            }

            if (string.IsNullOrEmpty(credentialsResponse?.Multifactor?.Email))
                return true;

            var mfaCode = await _alertService.PromptUserFor2FA(account, credentialsResponse.Multifactor.Email);

            await _httpClient.PutAsJsonAsync($"https://127.0.0.1:{port}/rso-auth/v1/session/multifactor", new RiotClientApi.MultifactorLoginResponse
            {
                Code = mfaCode,
                Retry = false,
                TrustDevice = true
            });

            return true;
        }

        private void CloseAllRiotApps()
        {
            foreach (var process in Process.GetProcesses())
                if (process.ProcessName.ToLower().Contains("league")
                    || process.ProcessName.ToLower().Contains("riot")
                    || process.ProcessName.ToLower().Contains("valorant"))
                    process.Kill();
        }

        private string GetRiotExePath()
        {
            var exePath = @$"{_settingsService.Settings.RiotInstallDirectory}\Riot Client\RiotClientServices.exe";
            if (!File.Exists(exePath))
                throw new RiotClientNotFoundException();

            return exePath;
        }

        private async Task<bool> VerifyLogInStatus()
        {
            await _riotFileSystemService.WaitForClientInit();

            if (!_riotService.TryGetPortAndToken(out var token, out var port))
                return false;

            _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", Convert.ToBase64String(System.Text.Encoding.ASCII.GetBytes($"riot:{token}")));
            var authorizationStatus = await _httpClient.GetAsync($"https://127.0.0.1:{port}/rso-auth/v1/authorization");
            if (!authorizationStatus.IsSuccessStatusCode)
                return false;

            return true;
        }

        private void StartLeague()
        {
            _logger.LogInformation("Launching league of legends...");

            var startLeagueCommandline = "--launch-product=league_of_legends --launch-patchline=live";
            var startLeague = new ProcessStartInfo
            {
                FileName = GetRiotExePath(),
                Arguments = startLeagueCommandline
            };
            Process.Start(startLeague);
        }
    }
}
