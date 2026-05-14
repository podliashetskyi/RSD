#pragma warning disable S1144, S4487, S2933

using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Components;
using RSD.Web.Services.Auth;

namespace RSD.Web.Components.Admin.Pages.Users;

public partial class InviteUser(IUserAdminService UserAdmin, NavigationManager Nav) : ComponentBase
{
    private InviteInput Input { get; set; } = new();
    private InviteResult? Sent { get; set; }
    private string ErrorMessage { get; set; } = "";

    private async Task SubmitAsync()
    {
        ErrorMessage = "";
        var baseUrl = Nav.BaseUri.TrimEnd('/');
        var result = await UserAdmin.InviteAsync(Input.Email, Input.DisplayName, baseUrl, CancellationToken.None);
        if (result.Ok) Sent = result.Value;
        else ErrorMessage = result.Error;
    }

    private void ResetForm()
    {
        Input = new InviteInput();
        Sent = null;
        ErrorMessage = "";
    }

    public sealed record class InviteInput
    {
        [Required, EmailAddress]
        public string Email { get; set; } = "";

        public string DisplayName { get; set; } = "";
    }
}
