using Microsoft.AspNetCore.Components;
using AccountManager.Core.Enums;
using System.Reflection;
using AccountManager.Core.Interfaces;
using Microsoft.Extensions.Caching.Memory;

namespace AccountManager.Blazor.Components.AccountListTile
{
    public partial class TileTopBar
    {
        [Parameter]
        public string Title { get; set; } = "";

        [Parameter]
        public AccountType AccountType { get; set; }

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

        public string GetLogoUrl()
        {
            return _memoryCache?.GetOrCreate($"{nameof(GetLogoUrl)}.{AccountType}", (entry) =>
            {
                var platformService = _platformServiceFactory?.CreateImplementation(AccountType);

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