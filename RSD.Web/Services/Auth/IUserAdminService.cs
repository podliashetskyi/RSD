using RSD.Web.Services.Common;

namespace RSD.Web.Services.Auth;

public sealed record AdminUserRow(
    string Id,
    string Email,
    string DisplayName,
    DateTime? LastLoginAt,
    bool IsDisabled);

public sealed record InviteResult(string Email, string ResetUrl);

public interface IUserAdminService
{
    Task<IReadOnlyList<AdminUserRow>> ListAsync(CancellationToken ct);
    Task<Result<InviteResult>> InviteAsync(string email, string displayName, string baseUrl, CancellationToken ct);
    Task<Result<Unit>> DisableAsync(string targetUserId, string currentUserId, CancellationToken ct);
    Task<Result<Unit>> EnableAsync(string targetUserId, CancellationToken ct);
    Task<Result<InviteResult>> ResendResetAsync(string targetUserId, string baseUrl, CancellationToken ct);
}
