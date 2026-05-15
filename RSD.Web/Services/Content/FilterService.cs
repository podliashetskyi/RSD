using Microsoft.EntityFrameworkCore;
using RSD.Web.Data;
using RSD.Web.Data.Entities;
using RSD.Web.Services.Cache;
using RSD.Web.Services.Slugs;
using RSD.Web.Services.Storage;

namespace RSD.Web.Services.Content;

public sealed class FilterService(
    IDbContextFactory<AppDbContext> DbFactory,
    ISlugger Slugger,
    IPublicPageCache Cache,
    IFileRefCountTracker RefCounts)
    : SimpleContentService<Filter>(DbFactory, Slugger, Cache, RefCounts), IFilterService
{
    // Filter slugs must be unique across types (the table is one bucket), so the
    // natural-key seed prefixes the label with the type. SimpleContentService's
    // EnsureSlugAsync will run this through Slugger to produce the final slug.
    protected override string NaturalKeyOf(Filter entity) => $"{entity.Type}-{entity.Label}";

    public async Task<IReadOnlyList<Filter>> ListByTypeAsync(FilterType type, CancellationToken ct)
    {
        await using var db = await DbFactory.CreateDbContextAsync(ct);
        return await db.Set<Filter>()
            .AsNoTracking()
            .Where(f => f.Type == type && f.Status == ContentStatus.Published)
            .OrderBy(f => f.DisplayOrder)
            .ThenBy(f => f.Label)
            .ToListAsync(ct);
    }
}
