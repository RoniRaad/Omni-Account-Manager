namespace AccountManager.Core.Models.RiotGames.League
{
    public sealed class UserChampSelectHistory
    {
        public IEnumerable<ChampSelectedCount> Champs { get; set; } = new List<ChampSelectedCount>();
    }
}