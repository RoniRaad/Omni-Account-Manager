using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AccountManager.Core.Services
{
    public class AlertService
    {
        private string errorMessage = "";
        private string infoMessage = "";
        public Action UpdateView { get; set; }
        public string ErrorMessage
        {
            get
            {
                return this.errorMessage;
            }
            set
            {
                errorMessage = value;
                UpdateView();
            }
        }
        public string InfoMessage
        {
            get
            {
                return this.infoMessage;
            }
            set
            {
                infoMessage = value;
                UpdateView();
            }
        }
    }
}
