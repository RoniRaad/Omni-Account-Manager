using System.Collections.Immutable;

namespace AccountManager.Core.Models.RiotGames.Valorant
{
    public sealed class ValorantRank : Rank
	{
		public static readonly IDictionary<string, string> RankedColorMap = new Dictionary<string, string>()
		{
			{"unrated", "#ffffff"},
			{"iron", "#000000"},
			{"bronze", "#ab370d"},
			{"silver", "#999c9b"},
			{"gold", "#e2cd5f"},
			{"platinum", "#32a4bb"},
			{"diamond", "#f195f4"},
			{"ascendant", "#3b9d72"},
			{"immortal", "#ac3654"},
		}.ToImmutableDictionary();
		
		public static readonly IList<string> RankMap = new List<string>() 
		{
			"UNRATED",
			"IRON",
			"BRONZE",
			"SILVER" ,
			"GOLD" ,
			"PLATINUM" ,
			"DIAMOND" ,
			"ASCENDANT" ,
			"RADIANT"
		}.ToImmutableList();
	}
}
