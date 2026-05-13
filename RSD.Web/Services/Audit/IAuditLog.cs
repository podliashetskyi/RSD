using RSD.Web.Data.Entities;

namespace RSD.Web.Services.Audit;

public interface IAuditLog
{
    Task<IReadOnlyList<AuditLogEntry>> ListAsync(AuditQuery query, CancellationToken ct);
}
