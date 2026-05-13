#pragma warning disable S1144, S4487, S2933

using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Identity;
using RSD.Web.Services.Auth;
using RSD.Web.Services.Email;
using RSD.Web.Services.Email.EmailTemplates;

namespace RSD.Web.Components.Admin.Pages;

public partial class ForgotPassword(
    UserManager<AdminUser> Users,
    IEmailSender Email,
    NavigationManager Nav)
{
    [SupplyParameterFromForm] private ForgotInput Input { get; set; } = new();
    private bool Submitted { get; set; }

    private async Task HandleForgotAsync()
    {
        var user = await Users.FindByEmailAsync(Input.Email);
        if (user is not null) await SendResetEmailAsync(user);
        Submitted = true;
    }

    private async Task SendResetEmailAsync(AdminUser user)
    {
        var token = await Users.GeneratePasswordResetTokenAsync(user);
        var resetUrl = BuildResetUrl(user.Email ?? "", token);
        var message = ForgotPasswordTemplate.Render(user.Email ?? "", user.DisplayName, resetUrl);
        await Email.SendAsync(message, CancellationToken.None);
    }

    private string BuildResetUrl(string email, string token)
    {
        var baseUri = Nav.BaseUri.TrimEnd('/');
        var encodedEmail = Uri.EscapeDataString(email);
        var encodedToken = Uri.EscapeDataString(token);
        return $"{baseUri}/admin/reset-password?email={encodedEmail}&token={encodedToken}";
    }

    public sealed record class ForgotInput
    {
        [Required, EmailAddress]
        public string Email { get; set; } = "";
    }
}
