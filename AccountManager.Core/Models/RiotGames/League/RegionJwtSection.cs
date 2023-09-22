using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace AccountManager.Core.Models.RiotGames.League
{
    public class RegionJwtSection
    {
        [JsonPropertyName("locales")]
        public List<string> Locales { get; set; }

        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("tag")]
        public string Tag { get; set; }
    }


}
