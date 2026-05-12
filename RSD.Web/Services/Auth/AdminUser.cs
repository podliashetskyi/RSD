using Microsoft.AspNetCore.Identity;

namespace RSD.Web.Services.Auth;

public class AdminUser : IdentityUser
{
    public string DisplayName { get; set; } = "";
}
