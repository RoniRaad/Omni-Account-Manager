using AccountManager.Core.Enums;
using AccountManager.Core.Factories;
using AccountManager.Core.Interfaces;
using AccountManager.Core.Models;
using AccountManager.Core.Models.RiotGames.Requests;
using AccountManager.Core.Services;
using AccountManager.Infrastructure.Services.FileSystem;
using Microsoft.Extensions.Caching.Memory;
using System.Diagnostics;

namespace AccountManager.Infrastructure.Services.Platform
{
    public class ValorantPlatformService : IPlatformService
    {
        private readonly ITokenService _riotService;
        private readonly IRiotClient _riotClient;
        private readonly RiotFileSystemService _riotFileSystemService;
        private readonly AlertService _alertService;
        private readonly IMemoryCache _memoryCache;
        private readonly HttpClient _httpClient;
        private readonly Dictionary<string, string> RankColorMap = new Dictionary<string, string>()
        {
            {"iron", "#000000"},
            {"bronze", "#ab370d"},
            {"silver", "#999c9b"},
            {"gold", "#e2cd5f"},
            {"platinum", "#32a4bb"},
            {"diamond", "#f195f4"},
            {"immortal", "#ac3654"},
        };
        public ValorantPlatformService(IRiotClient riotClient, GenericFactory<AccountType, ITokenService> tokenServiceFactory, 
            IHttpClientFactory httpClientFactory, RiotFileSystemService riotLockFileService, AlertService alertService, IMemoryCache memoryCache)
        {
            _riotClient = riotClient;
            _riotService = tokenServiceFactory.CreateImplementation(AccountType.Valorant);
            _httpClient = httpClientFactory.CreateClient("SSLBypass");
            _riotFileSystemService = riotLockFileService;
            _alertService = alertService;
            _memoryCache = memoryCache;
        }

        public async Task Login(Account account)
        {
            try
            {
                foreach (var process in Process.GetProcesses())
                    if (process.ProcessName.Contains("League") || process.ProcessName.Contains("Riot"))
                        process.Kill();

                await _riotFileSystemService.WaitForClientClose();
                _riotFileSystemService.DeleteLockfile();

                var request = new RiotSessionRequest
                {
                    Id = "riot-client",
                    Nonce = "1",
                    RedirectUri = "http://localhost/redirect",
                    ResponseType = "token id_token",
                    Scope = "openid offline_access lol ban profile email phone account"
                };

                var authResponse = await _riotClient.RiotAuthenticate(request, account);
                if (authResponse is null)
                    return;

                await _riotFileSystemService.WriteRiotYaml("NA", authResponse?.Cookies?.Tdid?.Value ?? "", authResponse?.Cookies?.Ssid?.Value ?? "",
                    authResponse?.Cookies?.Sub?.Value ?? "", authResponse?.Cookies?.Csid?.Value ?? "");

                StartValorant();
            }
            catch
            {
                _alertService.AddErrorMessage("There was an error signing in.");
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
            var rankCacheString = $"{account.Username}.valorant.rank";
            if (_memoryCache.TryGetValue(rankCacheString, out Rank? rank) && rank is not null)
                return (true, rank);

            rank = new Rank();
            try
            {
                if (string.IsNullOrEmpty(account.PlatformId))
                    account.PlatformId = await _riotClient.GetPuuId(account.Username, account.Password);
                if (string.IsNullOrEmpty(account.PlatformId))
                    return (false, rank);

                rank = await _riotClient.GetValorantRank(account);
                SetRankColor(rank);

                if (!string.IsNullOrEmpty(rank?.Tier))
                    _memoryCache.Set(rankCacheString, rank, TimeSpan.FromHours(1));

                if (rank is null)
                    return (false, new());

                return new(true, rank);
            }
            catch
            {
                return new(false, new ());
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

                var id = await _riotClient.GetPuuId(account.Username, account.Password);
                return new(id is not null, id ?? string.Empty);
            }
            catch
            {
                return new (false, string.Empty);
            }
        }

        private void SetRankColor(Rank rank)
        {
            rank.Color = RankColorMap.FirstOrDefault((kvp) => rank?.Tier?.ToLower()?.Equals(kvp.Key) is true, 
                new KeyValuePair<string, string>("", "")).Value;
        }

        private DriveInfo? FindRiotDrive()
        {
            DriveInfo? riotDrive = DriveInfo.GetDrives().FirstOrDefault(
                (drive) => Directory.Exists($"{drive?.RootDirectory}\\Riot Games"), null);

            return riotDrive;
        }

        private string GetRiotExePath()
        {
            return @$"{FindRiotDrive()?.RootDirectory}\Riot Games\Riot Client\RiotClientServices.exe";
        }

        public async Task<(bool, List<RankedGraphData>)> TryFetchRankedGraphData(Account account)
        {
            var matchHistory = await _riotClient.GetValorantCompetitiveHistory(account);
            if (matchHistory?.Matches?.Any() is not true)
                return new(false, new List<RankedGraphData>());

            var graphData = new RankedGraphData()
            {
                Data = matchHistory.Matches.Select((match) =>
                {
                    return new CoordinatePair()
                    {
                        Y = match.RankedRatingAfterUpdate,
                        X = match.MatchStartTime
                    };
                }).ToList(),
                Tags = new(),
                Label = "Competitive LP"
            };

            return (true, new List<RankedGraphData>() { graphData });
        }
    }
}
