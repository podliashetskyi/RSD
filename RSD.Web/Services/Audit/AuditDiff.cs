using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using RSD.Web.Data.Entities;

namespace RSD.Web.Services.Audit;

public static class AuditDiff
{
    private static readonly HashSet<string> ExcludedProperties =
    [
        "PasswordHash",
        "SecurityStamp",
        "ConcurrencyStamp",
        "NormalizedEmail",
        "NormalizedUserName",
    ];

    public static string ForAdded(EntityEntry entry) =>
        SerializeChanges(entry.Properties
            .Where(IsAuditable)
            .Select(p => new PropertyChange(p.Metadata.Name, null, p.CurrentValue)));

    public static string ForModified(EntityEntry entry) =>
        SerializeChanges(entry.Properties
            .Where(p => p.IsModified && IsAuditable(p))
            .Select(p => new PropertyChange(p.Metadata.Name, p.OriginalValue, p.CurrentValue)));

    public static string ForDeleted(EntityEntry entry) =>
        SerializeChanges(entry.Properties
            .Where(IsAuditable)
            .Select(p => new PropertyChange(p.Metadata.Name, p.OriginalValue, null)));

    public static AuditAction DeriveAction(EntityEntry entry)
    {
        if (entry.State == EntityState.Added) return AuditAction.Create;
        if (entry.State == EntityState.Deleted) return AuditAction.Delete;
        return DeriveActionForModified(entry);
    }

    private static AuditAction DeriveActionForModified(EntityEntry entry)
    {
        var deletion = DeletionTransition(entry);
        if (deletion is not null) return deletion.Value;
        return StatusTransition(entry) ?? AuditAction.Update;
    }

    private static AuditAction? DeletionTransition(EntityEntry entry)
    {
        var prop = entry.Properties.FirstOrDefault(p => p.Metadata.Name == nameof(ContentEntity.IsDeleted));
        return DeletionTransitionFromProperty(prop);
    }

    private static AuditAction? DeletionTransitionFromProperty(PropertyEntry? prop)
    {
        if (prop is null || !prop.IsModified) return null;
        return DeletionFromValues(ToBool(prop.OriginalValue), ToBool(prop.CurrentValue));
    }

    private static AuditAction? DeletionFromValues(bool was, bool now) => (was, now) switch
    {
        (false, true) => AuditAction.Delete,
        (true, false) => AuditAction.Restore,
        _ => null,
    };

    private static AuditAction? StatusTransition(EntityEntry entry)
    {
        var prop = entry.Properties.FirstOrDefault(p => p.Metadata.Name == nameof(ContentEntity.Status));
        return StatusTransitionFromProperty(prop);
    }

    private static AuditAction? StatusTransitionFromProperty(PropertyEntry? prop)
    {
        if (prop is null || !prop.IsModified) return null;
        return DeriveStatusAction(ToStatus(prop.OriginalValue), ToStatus(prop.CurrentValue));
    }

    private static AuditAction DeriveStatusAction(ContentStatus before, ContentStatus after)
    {
        if (IsPublishing(before, after)) return AuditAction.Publish;
        if (IsUnpublishing(before, after)) return AuditAction.Unpublish;
        if (IsArchiving(before, after)) return AuditAction.Archive;
        return AuditAction.Update;
    }

    private static bool IsPublishing(ContentStatus before, ContentStatus after) =>
        after == ContentStatus.Published && before != ContentStatus.Published;

    private static bool IsUnpublishing(ContentStatus before, ContentStatus after) =>
        before == ContentStatus.Published && after == ContentStatus.Draft;

    private static bool IsArchiving(ContentStatus before, ContentStatus after) =>
        after == ContentStatus.Archived && before != ContentStatus.Archived;

    private static bool IsAuditable(PropertyEntry p) => !ExcludedProperties.Contains(p.Metadata.Name);

    private static bool ToBool(object? value) => value is bool b && b;

    private static ContentStatus ToStatus(object? value) =>
        value is ContentStatus s ? s : ContentStatus.Draft;

    private static string SerializeChanges(IEnumerable<PropertyChange> changes)
    {
        var list = changes.ToList();
        if (list.Count == 0) return "{}";
        return JsonSerializer.Serialize(new { changes = list }, JsonSerializerOptions.Default);
    }

    private record PropertyChange(string Name, object? Before, object? After);
}
