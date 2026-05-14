using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using RSD.Web.Services.Common;
using RSD.Web.Services.Email;
using RSD.Web.Services.Email.EmailTemplates;

namespace RSD.Web.Services.Auth;

public sealed class UserAdminService(
    UserManager<AdminUser> Users,
    IEmailSender Email,
    ILogger<UserAdminService> Log) : IUserAdminService
{
    private static readonly DateTimeOffset DisabledUntil = DateTimeOffset.MaxValue;

    public async Task<IReadOnlyList<AdminUserRow>> ListAsync(CancellationToken ct)
    {
        var rows = await Users.Users
            .OrderBy(u => u.Email)
            .Select(u => new AdminUserRow(
                u.Id,
                u.Email ?? "",
                u.DisplayName,
                u.LastLoginAt,
                u.LockoutEnd != null && u.LockoutEnd > DateTimeOffset.UtcNow))
            .ToListAsync(ct);
        return rows;
    }

    public async Task<Result<InviteResult>> InviteAsync(string email, string displayName, string baseUrl, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(email)) return Result.Fail<InviteResult>("Email is required.");
        var existing = await Users.FindByEmailAsync(email);
        if (existing is not null) return Result.Fail<InviteResult>("A user with that email already exists.");

        var user = new AdminUser
        {
            UserName = email,
            Email = email,
            EmailConfirmed = true,
            DisplayName = string.IsNullOrWhiteSpace(displayName) ? email : displayName,
        };
        var create = await Users.CreateAsync(user);
        if (!create.Succeeded) return Result.Fail<InviteResult>(JoinErrors(create.Errors));

        var role = await Users.AddToRoleAsync(user, AdminRoles.Admin);
        if (!role.Succeeded)
        {
            await Users.DeleteAsync(user);
            return Result.Fail<InviteResult>(JoinErrors(role.Errors));
        }

        return await SendInviteAsync(user, baseUrl, ct);
    }

    public async Task<Result<Unit>> DisableAsync(string targetUserId, string currentUserId, CancellationToken ct)
    {
        if (string.Equals(targetUserId, currentUserId, StringComparison.Ordinal))
            return Result.Fail("You cannot disable your own account.");

        var user = await Users.FindByIdAsync(targetUserId);
        if (user is null) return Result.Fail("User not found.");

        var activeAdmins = await CountActiveAdminsAsync(ct);
        if (activeAdmins <= 1 && !IsCurrentlyDisabled(user))
            return Result.Fail("Cannot disable the last active admin.");

        var setLockout = await Users.SetLockoutEndDateAsync(user, DisabledUntil);
        if (!setLockout.Succeeded) return Result.Fail(JoinErrors(setLockout.Errors));

        // Invalidate any active session cookie (with SecurityStampValidator.ValidationInterval=0
        // this takes effect on their very next request).
        var stamp = await Users.UpdateSecurityStampAsync(user);
        if (!stamp.Succeeded) return Result.Fail(JoinErrors(stamp.Errors));
        return Result.Ok();
    }

    public async Task<Result<Unit>> EnableAsync(string targetUserId, CancellationToken ct)
    {
        var user = await Users.FindByIdAsync(targetUserId);
        if (user is null) return Result.Fail("User not found.");
        var result = await Users.SetLockoutEndDateAsync(user, null);
        return result.Succeeded ? Result.Ok() : Result.Fail(JoinErrors(result.Errors));
    }

    public async Task<Result<InviteResult>> ResendResetAsync(string targetUserId, string baseUrl, CancellationToken ct)
    {
        var user = await Users.FindByIdAsync(targetUserId);
        if (user is null) return Result.Fail<InviteResult>("User not found.");
        return await SendInviteAsync(user, baseUrl, ct);
    }

    private async Task<Result<InviteResult>> SendInviteAsync(AdminUser user, string baseUrl, CancellationToken ct)
    {
        var token = await Users.GeneratePasswordResetTokenAsync(user);
        var resetUrl = BuildResetUrl(baseUrl, user.Email ?? "", token);
        var message = InviteUserTemplate.Render(user.Email ?? "", user.DisplayName, resetUrl);
        try { await Email.SendAsync(message, ct); }
        catch (Exception ex)
        {
            Log.LogWarning(ex, "UserAdminService: invite email to '{Email}' failed to send; reset URL still available.", user.Email);
        }
        return Result.Ok(new InviteResult(user.Email ?? "", resetUrl));
    }

    private static bool IsCurrentlyDisabled(AdminUser user) =>
        user.LockoutEnd is { } end && end > DateTimeOffset.UtcNow;

    private async Task<int> CountActiveAdminsAsync(CancellationToken ct) =>
        await Users.Users
            .Where(u => u.LockoutEnd == null || u.LockoutEnd <= DateTimeOffset.UtcNow)
            .CountAsync(ct);

    private static string BuildResetUrl(string baseUrl, string email, string token)
    {
        var encodedEmail = Uri.EscapeDataString(email);
        var encodedToken = Uri.EscapeDataString(token);
        return $"{baseUrl.TrimEnd('/')}/admin/reset-password?email={encodedEmail}&token={encodedToken}";
    }

    private static string JoinErrors(IEnumerable<IdentityError> errors) =>
        string.Join("; ", errors.Select(e => e.Description));
}
