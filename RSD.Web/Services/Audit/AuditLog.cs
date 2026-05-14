using Microsoft.EntityFrameworkCore;
using RSD.Web.Data;
using RSD.Web.Data.Entities;

namespace RSD.Web.Services.Audit;

public sealed class AuditLog(AppDbContext Db) : IAuditLog
{
    public async Task<IReadOnlyList<AuditLogEntry>> ListAsync(AuditQuery query, CancellationToken ct)
    {
        var page = Math.Max(query.Page, 1);
        var size = Math.Clamp(query.PageSize, 1, 200);
        var q = Db.AuditLogEntries.AsNoTracking().AsQueryable();
        q = ApplyFilters(q, query);
        return await q.OrderByDescending(e => e.At)
                      .Skip((page - 1) * size)
                      .Take(size)
                      .ToListAsync(ct);
    }

    private static IQueryable<AuditLogEntry> ApplyFilters(IQueryable<AuditLogEntry> q, AuditQuery f)
    {
        q = ApplyUserAndType(q, f);
        q = ApplyAction(q, f);
        return ApplyDateRange(q, f);
    }

    private static IQueryable<AuditLogEntry> ApplyUserAndType(IQueryable<AuditLogEntry> q, AuditQuery f)
    {
        if (!string.IsNullOrEmpty(f.UserId)) q = q.Where(e => e.UserId == f.UserId);
        if (!string.IsNullOrEmpty(f.UserEmail)) q = q.Where(e => EF.Functions.ILike(e.UserEmail, $"%{f.UserEmail}%"));
        if (!string.IsNullOrEmpty(f.EntityType)) q = q.Where(e => e.EntityType == f.EntityType);
        return q;
    }

    private static IQueryable<AuditLogEntry> ApplyAction(IQueryable<AuditLogEntry> q, AuditQuery f) =>
        f.Action is { } action ? q.Where(e => e.Action == action) : q;

    private static IQueryable<AuditLogEntry> ApplyDateRange(IQueryable<AuditLogEntry> q, AuditQuery f)
    {
        if (f.From is { } from) q = q.Where(e => e.At >= from.ToDateTime(TimeOnly.MinValue));
        if (f.To is { } to) q = q.Where(e => e.At < to.AddDays(1).ToDateTime(TimeOnly.MinValue));
        return q;
    }
}
