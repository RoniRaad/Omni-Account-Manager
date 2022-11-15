namespace AccountManager.Core.Models.RiotGames
{
    public sealed class UserMatchHistory
    {
        public IEnumerable<GameMatch>? Matches { get; set; }
    }
}
