using AccountManager.Core.Models;
using AccountManager.Core.Models.EpicGames;

namespace AccountManager.Core.Interfaces
{
    public interface IEpicGamesTokenClient
    {
        Task<AccessTokenResponse?> GetAccessTokenAsync(string exchangeCode);
        Task<AccountInfo?> GetAccountInfo(string accessToken, string accountId);
        Task<string?> GetExchangeCode(string xsrfToken, string cookies);
    }
}