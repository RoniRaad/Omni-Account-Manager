
using AccountManager.Core.Enums;
using Microsoft.AspNetCore.Components;
using System;

namespace AccountManager.Blazor.Components
{
    public partial class FilterSidebar
    {
        [Parameter]
        public string Style { get; set; } = "";

        private void OnAccountTypeFilterCheckboxChange(ChangeEventArgs changeEventArgs, AccountType accountType)
        {
            if ((bool)(changeEventArgs?.Value ?? true)) 
                _accountFilterService.AccountTypeFilter.Add(accountType); 
            else 
                _accountFilterService.AccountTypeFilter.RemoveAll((type) => type == accountType);

            _accountFilterService.Save();
            InvokeAsync(StateHasChanged);
        }

        private void OnFilterInputChange(ChangeEventArgs e) 
        { 
            _accountFilterService.AccountNameFilter = e?.Value?.ToString() ?? string.Empty; 
            _accountFilterService.Save();
			InvokeAsync(StateHasChanged);
		}
	}
}