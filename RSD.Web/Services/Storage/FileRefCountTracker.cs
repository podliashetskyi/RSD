using Microsoft.EntityFrameworkCore;
using RSD.Web.Data;

namespace RSD.Web.Services.Storage;

public sealed class FileRefCountTracker(IDbContextFactory<AppDbContext> DbFactory) : IFileRefCountTracker
{
    public async Task ApplyDeltaAsync(IEnumerable<string> oldPaths, IEnumerable<string> newPaths, CancellationToken ct)
    {
        var oldSet = EntityPaths.Tracked(oldPaths).ToHashSet(StringComparer.Ordinal);
        var newSet = EntityPaths.Tracked(newPaths).ToHashSet(StringComparer.Ordinal);
        var added = newSet.Except(oldSet).ToList();
        var removed = oldSet.Except(newSet).ToList();
        if (added.Count == 0 && removed.Count == 0) return;

        await using var db = await DbFactory.CreateDbContextAsync(ct);
        await BumpAsync(db, added, +1, ct);
        await BumpAsync(db, removed, -1, ct);
        await db.SaveChangesAsync(ct);
    }

    private static async Task BumpAsync(AppDbContext db, List<string> paths, int delta, CancellationToken ct)
    {
        if (paths.Count == 0) return;
        var rows = await db.UploadedFiles.Where(f => paths.Contains(f.Path)).ToListAsync(ct);
        foreach (var row in rows)
        {
            row.RefCount = Math.Max(0, row.RefCount + delta);
        }
    }
}
