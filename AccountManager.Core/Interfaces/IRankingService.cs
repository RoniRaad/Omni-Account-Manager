using AccountManager.Core.ViewModels;

namespace AccountManager.Infrastructure.Services.RankingServices
{
    public interface IRankingService
    {
        Task<List<AccountListItemViewModel>> TryFetchRanks(List<AccountListItemViewModel> accounts);
        Task UpdateCurrentPuuid();
    }
}