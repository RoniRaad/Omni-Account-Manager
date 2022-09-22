using AccountManager.Core.Models.EpicGames;
using System;
using static AccountManager.UI.Services.EpicGamesExternalAuthService;

namespace AccountManager.UI.Services
{
    public sealed class EpicGamesCredentialsRecievedEventArgs : EventArgs
    {
        public EpicGamesLoginInfo? EpicGamesLoginInfo { get; set; }
    }
}
