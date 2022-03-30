using AccountManager.Core.Enums;
using AccountManager.Core.Factories;
using AccountManager.Core.Interfaces;
using AccountManager.Core.Models;
using AccountManager.Core.Models.RiotGames.League.Requests;
using AccountManager.Core.Services;
using AccountManager.Infrastructure.Services.FileSystem;
using System.Diagnostics;
using System.Net.Http.Json;
using System.Text.Json.Serialization;

namespace AccountManager.Infrastructure.Services.Platform
{
    public class ValorantPlatformService : IPlatformService
    {
        private readonly ITokenService _riotService;
        private readonly IRiotClient _riotClient;
        private readonly RiotLockFileService _riotFileSystemService;
        private readonly AlertService _alertService;
        private readonly HttpClient _httpClient;
        private Dictionary<string, string> RankColorMap = new Dictionary<string, string>()
        {
            {"iron", "#3a3a3a"},
            {"bronze", "#823012"},
            {"silver", "#999c9b"},
            {"gold", "#e2cd5f"},
            {"platinum", "#308798"},
            {"diamond", "#f195f4"},
            {"immortal", "#ac3654"},
        };
        public ValorantPlatformService(IRiotClient riotClient, GenericFactory<AccountType, ITokenService> tokenServiceFactory, 
            IHttpClientFactory httpClientFactory, RiotLockFileService riotLockFileService, AlertService alertService)
        {
            _riotClient = riotClient;
            _riotService = tokenServiceFactory.CreateImplementation(AccountType.Valorant);
            _httpClient = httpClientFactory.CreateClient("SSLBypass");
            _riotFileSystemService = riotLockFileService;
            _alertService = alertService;
        }
        public async Task Login(Account account)
        {
            try
            {
                foreach (var process in Process.GetProcesses())
                    if (process.ProcessName.Contains("League") || process.ProcessName.Contains("Riot"))
                        process.Kill();

                var request = new InitialAuthTokenRequest
                {
                    Id = "riot-client",
                    Nonce = "1",
                    RedirectUri = "http://localhost/redirect",
                    ResponseType = "token id_token",
                    Scope = "openid offline_access lol ban profile email phone account"
                };

                var authResponse = await _riotClient.GetRiotClientInitialCookies(request, account);
                if (authResponse.Cookies.Csid is null)
                    authResponse = await _riotClient.RiotAuthenticate(account, authResponse.Cookies);

                await _riotFileSystemService.WriteRiotYaml("NA", authResponse.Cookies.Tdid.Value, authResponse.Cookies.Ssid.Value,
                    authResponse.Cookies.Sub.Value, authResponse.Cookies.Csid.Value);

                StartValorant();
            }
            catch
            {
                _alertService.ErrorMessage = "There was an error signing in.";
            }
        }

        private void StartValorant()
        {
            var startValorantCommandline = "--launch-product=valorant --launch-patchline=live";
            var startValorant = new ProcessStartInfo
            {
                FileName = GetRiotExePath(),
                Arguments = startValorantCommandline
            };
            Process.Start(startValorant);
        }

        public async Task<(bool, Rank)> TryFetchRank(Account account)
        {
            Rank rank = new Rank();

            try
            {
                account.PlatformId ??= await _riotClient.GetPuuId(account.Username, account.Password);

                rank = await _riotClient.GetValorantRank(account);
                SetRankColor(rank);
                return new(true, rank);
            }
            catch
            {
                return new(false, rank);
            }
        }

        public async Task<(bool, string)> TryFetchId(Account account)
        {
            string id = "";

            try
            {
                if (!string.IsNullOrEmpty(account.PlatformId))
                {
                    return new (true, account.PlatformId);
                }

                id = await _riotClient.GetPuuId(account.Username, account.Password);
                return new(true, id);
            }
            catch
            {
                return new (false, id);
            }
        }
        private void SetRankColor(Rank rank)
        {
            foreach (KeyValuePair<string, string> kvp in RankColorMap)
                if (rank.Tier.ToLower().Equals(kvp.Key))
                    rank.Color = kvp.Value;
        }
        private DriveInfo FindRiotDrive()
        {
            DriveInfo riotDrive = null;
            foreach (DriveInfo drive in DriveInfo.GetDrives())
                if (Directory.Exists($"{drive.RootDirectory}\\Riot Games"))
                    riotDrive = drive;

            return riotDrive;
        }
        private string GetRiotExePath()
        {
            return @$"{FindRiotDrive().RootDirectory}\Riot Games\Riot Client\RiotClientServices.exe";
        }
    }
    public class Multifactor
    {
        [JsonPropertyName("email")]
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public string Email { get; set; }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

        [JsonPropertyName("method")]
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public string Method { get; set; }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

        [JsonPropertyName("methods")]
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public List<object> Methods { get; set; }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

        [JsonPropertyName("mfaVersion")]
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public object MfaVersion { get; set; }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

        [JsonPropertyName("multiFactorCodeLength")]
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public object MultiFactorCodeLength { get; set; }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    }

    public class RiotLoginResponse
    {
        [JsonPropertyName("authenticationType")]
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public string AuthenticationType { get; set; }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

        [JsonPropertyName("country")]
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public string Country { get; set; }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

        [JsonPropertyName("error")]
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public string Error { get; set; }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

        [JsonPropertyName("multifactor")]
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public Multifactor Multifactor { get; set; }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

        [JsonPropertyName("persistLogin")]
        public bool PersistLogin { get; set; }

        [JsonPropertyName("securityProfile")]
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public object SecurityProfile { get; set; }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

        [JsonPropertyName("type")]
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public string Type { get; set; }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    }
    public class MultifactorRequest
    {
        [JsonPropertyName("code")]
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public string Code { get; set; }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

        [JsonPropertyName("retry")]
        public bool Retry { get; set; }

        [JsonPropertyName("trustDevice")]
        public bool TrustDevice { get; set; }
    }

    public class CreateAuthorizations
    {
        [JsonPropertyName("authenticationType")]
        public string AuthenticationType { get; set; } = "SSOAuth";
        [JsonPropertyName("claims")]
        public List<string> Claims { get; set; } = new List<string>()
        {
            "sub","iss","auth_time","acr","name"
        };
        [JsonPropertyName("scope")]
        public List<string> Scope { get; set; } = new List<string>()
        {
            "openid","profile","email","lol","summoner"
        };
        [JsonPropertyName("clientId")]
        public string ClientId { get; set; } = "riot-client";

        [JsonPropertyName("keepAlive")]
        public bool KeepAlive { get; set; } = true;

        [JsonPropertyName("trustLevels")]
        public List<string> TrustLevels { get; set; } = new List<string>()
        {
            "always_trusted"
        };
    }



}
