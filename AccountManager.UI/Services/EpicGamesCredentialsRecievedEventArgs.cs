﻿using AccountManager.Core.Models;
using System;
using static AccountManager.UI.Services.EpicGamesExternalAuthService;

namespace AccountManager.UI.Services
{
    public class EpicGamesCredentialsRecievedEventArgs : EventArgs
    {
        public EpicGamesLoginInfo? EpicGamesLoginInfo { get; set; }
    }
}
