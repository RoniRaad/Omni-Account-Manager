using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AccountManager.Core.Static
{
    public static class CacheKeys
    {
        public static class LoginCacheKeys
        {
            public const string RememberMe = "rememberPassword";
            public const string RememberedPassword = "masterPassword";
        }
    }
}
