using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AccountManager.Core.Interfaces;
using AccountManager.Core.Models;
using AccountManager.Core.Models.UserSettings;
using AccountManager.Infrastructure.Services;
using Microsoft.AspNetCore.Components;

namespace AccountManager.Blazor.Components.Modals
{
    public partial class ChangePasswordModal
    {
        public PasswordChangeRequest PasswordChangeRequest { get; set; } = new();
        [Parameter, EditorRequired]
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public Action Close { get; set; }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        [Parameter, EditorRequired]
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public IUserSettingsService<GeneralSettings> SettingsService { get; set; }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

        public void Submit()
        {
            SettingsService.ChangePasswordAsync(PasswordChangeRequest);
            Close();
        }
    }
}