#pragma warning disable S1144, S4487, S2933

using Microsoft.AspNetCore.Components;

namespace RSD.Web.Components.Admin;

public partial class RedirectToLogin(NavigationManager Nav) : ComponentBase
{
    protected override void OnInitialized()
    {
        var returnUrl = Uri.EscapeDataString(new Uri(Nav.Uri).PathAndQuery);
        Nav.NavigateTo($"/admin/login?returnUrl={returnUrl}", forceLoad: true);
    }
}
