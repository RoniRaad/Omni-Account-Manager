using Microsoft.AspNetCore.Components;
using AccountManager.Core.Enums;
using System.Reflection;
using AccountManager.Core.Interfaces;
using Microsoft.Extensions.Caching.Memory;
using AccountManager.Core.Models;

namespace AccountManager.Blazor.Components.AccountListTile
{
    public partial class TileTopBar
    {
        [CascadingParameter]
        public Account? Account { get; set; }

        [Parameter]
        public EventCallback MouseEnterDragLogo { get; set; }

        [Parameter]
        public EventCallback MouseExitDragLogo { get; set; }
        private string logoUrl = "";

        protected override void OnInitialized()
        {
            logoUrl = GetLogoUrl();
            base.OnInitialized();
        }

        protected override void OnParametersSet()
        {
            logoUrl = GetLogoUrl();
            base.OnParametersSet();
        }

        public string GetLogoUrl()
        {
            if (Account is null)
                return "";

            return _memoryCache?.GetOrCreate($"{nameof(GetLogoUrl)}.{Account.AccountType}", (entry) =>
            {
                var platformService = _platformServiceFactory?.CreateImplementation(Account.AccountType);

                if (platformService is null)
                {
                    entry.AbsoluteExpiration = DateTimeOffset.Now;
                    return null;
                }

                return platformService.GetType()?.GetField("WebIconFilePath", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.FlattenHierarchy | BindingFlags.Public)?.GetValue(null)?.ToString();
            }) ?? "";
       }
    }
}