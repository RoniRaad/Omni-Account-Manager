using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace AccountManager.Core.Models
{
    public class CoordinatePair
    {
        [JsonPropertyName("y")]
        public int? Y { get; set; }
        [JsonPropertyName("x")]
        public double? X { get; set; }
    }
}
