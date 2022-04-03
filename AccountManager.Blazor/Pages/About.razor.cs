using System.Diagnostics;

namespace AccountManager.Blazor.Pages
{
    public partial class About
    {
        public void OpenGitHubPage()
        {
            Process.Start(new ProcessStartInfo(_aboutEndpointsOptions?.Value?.Github ?? "")
            {UseShellExecute = true});
        }
    }
}