namespace AccountManager.Core.Models
{
    public class GameMatch
    {
        public string? Id { get; set; }
        public bool Win { get; set; }
        public DateTimeOffset EndTime { get; set; }
        public string? Type { get; set; }
    }
}
