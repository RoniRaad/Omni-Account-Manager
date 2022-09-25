using Microsoft.Web.WebView2.Wpf;
using System.Windows;

namespace AccountManager.UI.Windows
{
    /// <summary>
    /// Interaction logic for WebView2Browser.xaml
    /// </summary>
    public partial class WebView2Browser : Window
    {
        public WebView2 WView2 { get; set; }
        public WebView2Browser()
        {
            InitializeComponent();
            WView2 = webv2;
        }
    }
}
