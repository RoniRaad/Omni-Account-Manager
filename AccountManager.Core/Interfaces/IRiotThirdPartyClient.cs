using AccountManager.Core.Models.RiotGames.Valorant;
using AccountManager.Core.Models.RiotGames.Valorant.Responses;

namespace AccountManager.Infrastructure.Clients
{
    public interface IRiotThirdPartyClient
    {
        Task<RiotVersionInfo?> GetRiotVersionInfoAsync();
        Task<ValorantOperatorsResponse> GetValorantOperators();
        Task<ValorantSkinLevelResponse> GetValorantSkinFromUuid(string uuid);
    }
}