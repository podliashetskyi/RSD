#pragma warning disable S1144, S4487, S2933

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using RSD.Web.Services.Auth;

namespace RSD.Web.Components.Admin.Layout;

public partial class AdminLayout(AuthenticationStateProvider AuthState) : LayoutComponentBase
{
    private string DisplayName { get; set; } = "";

    protected override async Task OnInitializedAsync()
    {
        var state = await AuthState.GetAuthenticationStateAsync();
        DisplayName = ResolveDisplayName(state.User);
    }

    private static string ResolveDisplayName(System.Security.Claims.ClaimsPrincipal user) =>
        user.FindFirst("displayname")?.Value ?? user.Identity?.Name ?? "";
}
