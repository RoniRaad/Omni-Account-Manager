using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AccountManager.Core.ViewModels
{
    public class AccountPageViewModel
    {
        public List<AccountListItemViewModel> AccountLists = new List<AccountListItemViewModel>();

        public bool AddAccountPromptShow = false;

    }
}
