using System.ComponentModel.DataAnnotations;

namespace AccountManager.Core.Models.UserSettings
{
    public sealed class LeagueSettings
    {
        public LeagueSettings() { }

        public bool UseAccountCredentials { get; set; } = true;
        public Guid? AccountToUseCredentials { get; set; }
        [Range(1, 40, ErrorMessage = "Matches must be within (1-40).")]
        public int AmountOfMatchesForGraphs { get; set; } = 20;
    }
}
