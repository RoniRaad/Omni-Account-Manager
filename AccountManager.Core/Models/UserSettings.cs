using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AccountManager.Core.Models
{
    public class UserSettings
    {
        public UserSettings(){ }
        public bool UseAccountCredentials { get; set; } = true;
    }
}
