namespace RSD.Web.Services.Content.Trash;

/// <summary>
/// Unified shape for any soft-deleted content row, regardless of source entity type.
/// EntityKey is the stable string used to route Restore/HardDelete back to the right service.
/// </summary>
public sealed record TrashItem(
    string EntityKey,
    string EntityLabel,
    Guid Id,
    string Title,
    string Slug,
    DateTime DeletedAt);
