#pragma warning disable S1144, S4487, S2933

using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Identity;
using RSD.Web.Services.Auth;

namespace RSD.Web.Components.Admin.Pages;

public partial class ResetPassword(UserManager<AdminUser> Users)
{
    [SupplyParameterFromQuery] private string? Email { get; set; }
    [SupplyParameterFromQuery] private string? Token { get; set; }
    [SupplyParameterFromForm] private ResetInput Input { get; set; } = new();

    private bool Succeeded { get; set; }
    private string ErrorMessage { get; set; } = "";

    protected override void OnInitialized()
    {
        Input.Email = Email ?? Input.Email;
        Input.Token = Token ?? Input.Token;
    }

    private async Task HandleResetAsync()
    {
        var user = await Users.FindByEmailAsync(Input.Email);
        if (user is null) { ErrorMessage = "Invalid reset link."; return; }
        var result = await Users.ResetPasswordAsync(user, Input.Token, Input.NewPassword);
        ApplyResult(result);
    }

    private void ApplyResult(IdentityResult result)
    {
        if (result.Succeeded) { Succeeded = true; return; }
        ErrorMessage = string.Join("; ", result.Errors.Select(e => e.Description));
    }

    public sealed record class ResetInput
    {
        public string Email { get; set; } = "";
        public string Token { get; set; } = "";

        [Required]
        public string NewPassword { get; set; } = "";

        [Required, Compare(nameof(NewPassword), ErrorMessage = "Passwords do not match.")]
        public string ConfirmPassword { get; set; } = "";
    }
}
