using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AccountManager.Core.Models
{
    public sealed class PasswordChangeRequest
    {
        public string OldPassword { get; set; } = string.Empty;
        public string NewPassword { get; set; } = string.Empty;
        public string NewPasswordConfirm { get; set; } = string.Empty;
    }
}
