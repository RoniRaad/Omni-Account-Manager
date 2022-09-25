
using System.Collections.Generic;
using System.Collections.Immutable;

namespace AccountManager.Core.Models.RiotGames.League
{
    public sealed class LeagueRank : Rank {
		public static readonly IDictionary<string, string> RankedColorMap = new Dictionary<string, string>()
		{
			{"unranked", "#ffffff"},
			{"iron", "#000000"},
			{"bronze", "#ac3d14"},
			{"silver", "#7e878b"},
			{"gold", "#FFD700"},
			{"platinum", "#25cb6e"},
			{"diamond", "#9e7ad6"},
			{"master", "#f359f9"},
			{"grandmaster", "#f8848f"},
			{"challenger", "#4ee1ff"},
		}.ToImmutableDictionary();
	}
    public sealed class TeamFightTacticsRank : Rank 
    {
		public static readonly IDictionary<string, string> RankedColorMap = new Dictionary<string, string>()
		{
			{"unranked", "#ffffff"},
			{"iron", "#000000"},
			{"bronze", "#ac3d14"},
			{"silver", "#7e878b"},
			{"gold", "#FFD700"},
			{"platinum", "#25cb6e"},
			{"diamond", "#9e7ad6"},
			{"master", "#f359f9"},
			{"grandmaster", "#f8848f"},
			{"challenger", "#4ee1ff"},
		}.ToImmutableDictionary();
	}
}
