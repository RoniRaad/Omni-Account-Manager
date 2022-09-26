namespace AccountManager.Core.Models
{
    public sealed class GameMatch
    {
        public string? Id { get; set; }
        public int GraphValueChange { get; set; }
        public DateTimeOffset EndTime { get; set; }
        public string? Type { get; set; }
    }
}
