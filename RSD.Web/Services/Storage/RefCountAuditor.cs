using Microsoft.EntityFrameworkCore;
using RSD.Web.Data;
using RSD.Web.Data.Entities;

namespace RSD.Web.Services.Storage;

/// <summary>
/// One-shot full recomputation of UploadedFile.RefCount values. Useful after seeding
/// or to correct drift. Not wired as a hosted background task by default; admins
/// trigger it on demand from the media library page.
/// </summary>
public sealed class RefCountAuditor(IDbContextFactory<AppDbContext> DbFactory)
{
    public async Task<int> RecomputeAllAsync(CancellationToken ct)
    {
        await using var db = await DbFactory.CreateDbContextAsync(ct);
        var counts = new Dictionary<string, int>(StringComparer.Ordinal);

        foreach (var e in await db.BlogPosts.IgnoreQueryFilters().ToListAsync(ct)) Tally(counts, EntityPaths.Of(e));
        foreach (var e in await db.Cases.IgnoreQueryFilters().ToListAsync(ct)) Tally(counts, EntityPaths.Of(e));
        foreach (var e in await db.Products.IgnoreQueryFilters().ToListAsync(ct)) Tally(counts, EntityPaths.Of(e));
        foreach (var e in await db.Services.IgnoreQueryFilters().ToListAsync(ct)) Tally(counts, EntityPaths.Of(e));
        foreach (var e in await db.Testimonials.IgnoreQueryFilters().ToListAsync(ct)) Tally(counts, EntityPaths.Of(e));
        foreach (var e in await db.TeamMembers.IgnoreQueryFilters().ToListAsync(ct)) Tally(counts, EntityPaths.Of(e));
        foreach (var e in await db.Partners.IgnoreQueryFilters().ToListAsync(ct)) Tally(counts, EntityPaths.Of(e));
        foreach (var e in await db.Values.IgnoreQueryFilters().ToListAsync(ct)) Tally(counts, EntityPaths.Of(e));
        foreach (var e in await db.MissionStats.IgnoreQueryFilters().ToListAsync(ct)) Tally(counts, EntityPaths.Of(e));
        foreach (var e in await db.TechStackItems.IgnoreQueryFilters().ToListAsync(ct)) Tally(counts, EntityPaths.Of(e));
        foreach (var e in await db.ContactPoints.IgnoreQueryFilters().ToListAsync(ct)) Tally(counts, EntityPaths.Of(e));
        foreach (var e in await db.MessengerLinks.IgnoreQueryFilters().ToListAsync(ct)) Tally(counts, EntityPaths.Of(e));
        foreach (var e in await db.SocialLinks.IgnoreQueryFilters().ToListAsync(ct)) Tally(counts, EntityPaths.Of(e));

        var files = await db.UploadedFiles.ToListAsync(ct);
        foreach (var file in files)
        {
            var newCount = counts.GetValueOrDefault(file.Path, 0);
            if (file.RefCount != newCount) file.RefCount = newCount;
        }
        await db.SaveChangesAsync(ct);
        return files.Count;
    }

    private static void Tally(Dictionary<string, int> counts, IEnumerable<string> paths)
    {
        foreach (var p in EntityPaths.Tracked(paths))
        {
            counts[p] = counts.GetValueOrDefault(p, 0) + 1;
        }
    }
}
