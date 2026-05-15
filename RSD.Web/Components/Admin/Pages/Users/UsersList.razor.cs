#pragma warning disable S1144, S4487, S2933

using System.Security.Claims;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using RSD.Web.Components.Admin.Shared;
using RSD.Web.Services.Auth;

namespace RSD.Web.Components.Admin.Pages.Users;

public partial class UsersList(
    IUserAdminService UserAdmin,
    AuthenticationStateProvider AuthState,
    NavigationManager Nav,
    IToastService Toasts) : ComponentBase
{
    private List<AdminUserRow> Rows { get; set; } = [];
    private string CurrentUserId { get; set; } = "";
    private string Message { get; set; } = "";
    private string MessageClasses { get; set; } = "";
    private string ResetUrl { get; set; } = "";

    protected override async Task OnInitializedAsync()
    {
        var state = await AuthState.GetAuthenticationStateAsync();
        CurrentUserId = state.User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "";
        await ReloadAsync();
    }

    private async Task ReloadAsync()
    {
        var rows = await UserAdmin.ListAsync(CancellationToken.None);
        Rows = [.. rows];
    }

    private async Task DisableAsync(string id)
    {
        var result = await UserAdmin.DisableAsync(id, CurrentUserId, CancellationToken.None);
        ApplyToast(result.Ok, result.Ok ? "User disabled." : result.Error);
        await ReloadAsync();
    }

    private async Task EnableAsync(string id)
    {
        var result = await UserAdmin.EnableAsync(id, CancellationToken.None);
        ApplyToast(result.Ok, result.Ok ? "User enabled." : result.Error);
        await ReloadAsync();
    }

    private async Task ResendAsync(string id)
    {
        var baseUrl = Nav.BaseUri.TrimEnd('/');
        var result = await UserAdmin.ResendResetAsync(id, baseUrl, CancellationToken.None);
        if (result.Ok && result.Value is { } invite)
        {
            ShowInline("info", $"Password-reset link sent to {invite.Email}.", invite.ResetUrl);
            Toasts.Show("Reset email sent.", ToastKind.Success);
        }
        else
        {
            ApplyToast(false, result.Error);
        }
    }

    private void ApplyToast(bool ok, string text) =>
        Toasts.Show(text, ok ? ToastKind.Success : ToastKind.Error);

    private static string StatusLabel(AdminUserRow row) =>
        row.IsDisabled ? "Disabled"
        : row.LastLoginAt is null ? "Invited"
        : "Active";

    private static string StatusBadgeClasses(AdminUserRow row) =>
        row.IsDisabled ? "bg-red-100 text-red-700 dark:bg-red-950/40 dark:text-red-300"
        : row.LastLoginAt is null ? "bg-amber-100 text-amber-700 dark:bg-amber-950/40 dark:text-amber-300"
        : "bg-emerald-100 text-emerald-700 dark:bg-emerald-950/40 dark:text-emerald-300";

    private void ShowInline(string kind, string text, string url)
    {
        Message = text;
        ResetUrl = url;
        MessageClasses = kind switch
        {
            "info" => "border-blue-200 bg-blue-50 text-blue-700 dark:border-blue-900 dark:bg-blue-950 dark:text-blue-300",
            "error" => "border-red-200 bg-red-50 text-red-700 dark:border-red-900 dark:bg-red-950 dark:text-red-300",
            _ => "border-gray-200 bg-gray-50 text-gray-700 dark:border-gray-800 dark:bg-gray-900 dark:text-gray-300",
        };
    }
}
