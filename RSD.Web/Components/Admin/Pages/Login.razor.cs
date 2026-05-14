#pragma warning disable S1144, S4487, S2933

using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Identity;
using RSD.Web.Services.Auth;

namespace RSD.Web.Components.Admin.Pages;

public partial class Login(
    SignInManager<AdminUser> SignIn,
    UserManager<AdminUser> Users,
    NavigationManager Nav)
{
    [SupplyParameterFromForm] private LoginInput Input { get; set; } = new();
    [SupplyParameterFromQuery] private string? ReturnUrl { get; set; }
    private string ErrorMessage { get; set; } = "";

    private async Task HandleSignInAsync()
    {
        var result = await SignIn.PasswordSignInAsync(Input.Email, Input.Password, Input.RememberMe, lockoutOnFailure: true);
        ErrorMessage = ResolveError(result);
        if (result.Succeeded)
        {
            await RecordLoginAsync();
            Nav.NavigateTo(ResolveRedirect(), forceLoad: true);
        }
    }

    private async Task RecordLoginAsync()
    {
        var user = await Users.FindByEmailAsync(Input.Email);
        if (user is null) return;
        user.LastLoginAt = DateTime.UtcNow;
        await Users.UpdateAsync(user);
    }

    private string ResolveRedirect()
    {
        if (string.IsNullOrWhiteSpace(ReturnUrl)) return "/admin";
        return Nav.ToAbsoluteUri(ReturnUrl).Host == new Uri(Nav.BaseUri).Host ? ReturnUrl : "/admin";
    }

    private static string ResolveError(SignInResult result)
    {
        if (result.Succeeded) return "";
        if (result.IsLockedOut) return "This account is locked. Try again in 15 minutes.";
        if (result.IsNotAllowed) return "This account is not allowed to sign in.";
        return "Email or password is incorrect.";
    }

    public sealed record class LoginInput
    {
        [Required, EmailAddress]
        public string Email { get; set; } = "";

        [Required]
        public string Password { get; set; } = "";

        public bool RememberMe { get; set; }
    }
}
