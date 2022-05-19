using AccountManager.Core.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AccountManager.Core.Models
{
    public class Account
    {
        public Guid Guid { get; set; } = Guid.NewGuid();
        public string? Id { get; set; } // Non-Unique ID
        public string? PlatformId { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public Rank? Rank { get; set; }
        public AccountType AccountType { get; set; }
        public List<RankedGraphData> Graphs { get; set; }
    }
}
