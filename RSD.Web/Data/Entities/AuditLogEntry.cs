namespace RSD.Web.Data.Entities;

public record class AuditLogEntry
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public string UserId { get; init; } = "";
    public string UserEmail { get; init; } = "";
    public required string EntityType { get; init; }
    public Guid EntityId { get; init; }
    public AuditAction Action { get; init; }
    public string Diff { get; init; } = "{}";
    public DateTime At { get; init; } = DateTime.UtcNow;
}
