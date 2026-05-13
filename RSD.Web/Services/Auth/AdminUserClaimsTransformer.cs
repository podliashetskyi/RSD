using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;

namespace RSD.Web.Services.Auth;

public sealed class AdminUserClaimsTransformer(UserManager<AdminUser> Users) : IClaimsTransformation
{
    private const string DisplayNameClaim = "displayname";

    public async Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
    {
        if (!IsAuthenticated(principal)) return principal;
        if (HasDisplayNameClaim(principal)) return principal;
        return await AppendDisplayNameAsync(principal);
    }

    private static bool IsAuthenticated(ClaimsPrincipal principal) =>
        principal.Identity?.IsAuthenticated == true;

    private static bool HasDisplayNameClaim(ClaimsPrincipal principal) =>
        principal.HasClaim(c => c.Type == DisplayNameClaim);

    private async Task<ClaimsPrincipal> AppendDisplayNameAsync(ClaimsPrincipal principal)
    {
        var user = await Users.GetUserAsync(principal);
        if (user is null || string.IsNullOrEmpty(user.DisplayName)) return principal;
        var identity = (ClaimsIdentity)principal.Identity!;
        identity.AddClaim(new Claim(DisplayNameClaim, user.DisplayName));
        return principal;
    }
}
