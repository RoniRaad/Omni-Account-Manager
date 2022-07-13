using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AccountManager.Core.Models.Steam
{
    public class SteamUsers
    {
        public Dictionary<string, SteamUser>? Users { get; set; }
    }

    public class SteamUser
    {
        public string AccountName { get; set; } = "";
        public string PersonaName { get; set; } = "";
    }
}
