using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;
using RSD.Web.Data.Entities;
using RSD.Web.Services.Audit;

namespace RSD.Web.Data.Interceptors;

public sealed class AuditSaveChangesInterceptor(
    IHttpContextAccessor HttpAccessor,
    ILogger<AuditSaveChangesInterceptor> Log) : SaveChangesInterceptor
{
    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        var ctx = eventData.Context;
        if (ctx is not null) TryWriteAudit(ctx);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private void TryWriteAudit(DbContext ctx)
    {
        try
        {
            WriteAudit(ctx);
        }
        catch (Exception ex)
        {
            Log.LogWarning(ex, "Audit interceptor failed; the save will still commit.");
        }
    }

    private void WriteAudit(DbContext ctx)
    {
        var (userId, userEmail) = ResolveUser();
        var entries = ctx.ChangeTracker.Entries()
            .Where(IsAuditable)
            .Select(e => Build(e, userId, userEmail))
            .ToList();
        if (entries.Count > 0) ctx.AddRange(entries);
    }

    private (string UserId, string UserEmail) ResolveUser()
    {
        var principal = HttpAccessor.HttpContext?.User;
        if (!IsAuthenticated(principal)) return ("", "");
        return (FirstClaim(principal, ClaimTypes.NameIdentifier), FirstClaim(principal, ClaimTypes.Email));
    }

    private static bool IsAuthenticated(ClaimsPrincipal? principal) =>
        principal?.Identity?.IsAuthenticated == true;

    private static string FirstClaim(ClaimsPrincipal? principal, string claimType) =>
        principal?.FindFirstValue(claimType) ?? "";

    private static bool IsAuditable(EntityEntry e)
    {
        if (IsIgnoredState(e.State)) return false;
        if (e.Entity is AuditLogEntry) return false;
        return IsTrackedEntity(e.Entity);
    }

    private static bool IsIgnoredState(EntityState state) =>
        state is EntityState.Detached or EntityState.Unchanged;

    private static bool IsTrackedEntity(object entity) =>
        entity is ContentEntity or ContactSubmission;

    private static AuditLogEntry Build(EntityEntry entry, string userId, string userEmail) => new()
    {
        UserId = userId,
        UserEmail = userEmail,
        EntityType = entry.Entity.GetType().Name,
        EntityId = ExtractId(entry),
        Action = AuditDiff.DeriveAction(entry),
        Diff = BuildDiff(entry),
        At = DateTime.UtcNow,
    };

    private static Guid ExtractId(EntityEntry entry)
    {
        var idProp = entry.Properties.FirstOrDefault(p => p.Metadata.IsPrimaryKey());
        return idProp?.CurrentValue is Guid g ? g : Guid.Empty;
    }

    private static string BuildDiff(EntityEntry entry) => entry.State switch
    {
        EntityState.Added => AuditDiff.ForAdded(entry),
        EntityState.Deleted => AuditDiff.ForDeleted(entry),
        _ => AuditDiff.ForModified(entry),
    };
}
