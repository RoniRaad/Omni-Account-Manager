using AccountManager.Core.Models;
using AccountManager.Core.Models.RiotGames.Valorant.Responses;

namespace AccountManager.Infrastructure.Clients
{
    public interface IValorantClient
    {
        Task<ValorantRankedHistoryResponse?> GetValorantCompetitiveHistory(Account account);
        Task<IEnumerable<ValorantMatch>?> GetValorantGameHistory(Account account);
        Task<Rank> GetValorantRank(Account account);
        Task<List<ValorantSkinLevelResponse>> GetValorantShopDeals(Account account);
        Task<string?> GetValorantToken(Account account);
    }
}