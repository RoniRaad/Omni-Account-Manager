using System.Diagnostics;

namespace AccountManager.Blazor.Pages
{
    public partial class About
    {
        public void OpenGitHubPage()
        {
            Process.Start(new ProcessStartInfo("https://github.com/RoniRaad/Multi-Game-Account-Manager".Replace("&", "^&"))
            {UseShellExecute = true});
        }
    }
}