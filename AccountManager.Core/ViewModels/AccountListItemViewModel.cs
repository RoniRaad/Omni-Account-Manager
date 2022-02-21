using AccountManager.Core.Interfaces;
using AccountManager.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AccountManager.Core.ViewModels
{
    public class AccountListItemViewModel
    {
        public string Name { get; set; }
        public Account Account { get; set; }
        public ILoginService LoginService { get; set; }
    }
}
