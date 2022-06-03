using AccountManager.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AccountManager.Core.Models.RiotGames.Valorant
{
    public class ValorantRank : Rank
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
			"IMMORTAL" ,
			"RADIANT"
		}.ToImmutableList();
	}
}
