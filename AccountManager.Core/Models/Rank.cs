using AccountManager.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AccountManager.Core.Models
{
    public class Rank
    {
        public string? Ranking { get; set; } = string.Empty;
        public string? Tier { get; set; } = string.Empty;
        public string? HexColor { get; set; } = string.Empty;
    }
}
