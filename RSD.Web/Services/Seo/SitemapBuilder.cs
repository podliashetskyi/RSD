using Microsoft.EntityFrameworkCore;
using RSD.Web.Data;
using RSD.Web.Data.Entities;

namespace RSD.Web.Services.Seo;

public sealed class SitemapBuilder(IDbContextFactory<AppDbContext> DbFactory) : ISitemapBuilder
{
    private static readonly DateTime FallbackLastMod = new(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    public async Task<IReadOnlyList<SitemapEntry>> BuildAsync(string baseUrl, CancellationToken ct)
    {
        var root = baseUrl.TrimEnd('/');
        await using var db = await DbFactory.CreateDbContextAsync(ct);

        var entries = new List<SitemapEntry>
        {
            new($"{root}/", FallbackLastMod),
            new($"{root}/blog", await NewestPublishedAsync(db.BlogPosts, ct)),
            new($"{root}/cases", await NewestPublishedAsync(db.Cases, ct)),
            new($"{root}/products", FallbackLastMod),
            new($"{root}/services", FallbackLastMod),
            new($"{root}/contact", FallbackLastMod),
            new($"{root}/about", FallbackLastMod),
            new($"{root}/estimate", FallbackLastMod),
        };

        entries.AddRange(await db.TermsOfService.AsNoTracking()
            .Where(e => e.Status == ContentStatus.Published)
            .Select(e => new SitemapEntry($"{root}/terms-of-service", e.UpdatedAt))
            .ToListAsync(ct));
        entries.AddRange(await db.PrivacyPolicies.AsNoTracking()
            .Where(e => e.Status == ContentStatus.Published)
            .Select(e => new SitemapEntry($"{root}/privacy-policy", e.UpdatedAt))
            .ToListAsync(ct));

        entries.AddRange(await db.BlogPosts.AsNoTracking()
            .Where(e => e.Status == ContentStatus.Published)
            .Select(e => new SitemapEntry($"{root}/blog/{e.Slug}", e.UpdatedAt))
            .ToListAsync(ct));
        entries.AddRange(await db.Cases.AsNoTracking()
            .Where(e => e.Status == ContentStatus.Published)
            .Select(e => new SitemapEntry($"{root}/cases/{e.Slug}", e.UpdatedAt))
            .ToListAsync(ct));
        entries.AddRange(await db.Products.AsNoTracking()
            .Where(e => e.Status == ContentStatus.Published)
            .Select(e => new SitemapEntry($"{root}/products/{e.Slug}", e.UpdatedAt))
            .ToListAsync(ct));
        entries.AddRange(await db.Services.AsNoTracking()
            .Where(e => e.Status == ContentStatus.Published)
            .Select(e => new SitemapEntry($"{root}/services/{e.Slug}", e.UpdatedAt))
            .ToListAsync(ct));

        return entries;
    }

    // Listing roots advertise the freshness of their newest published child, not a frozen date.
    private static async Task<DateTime> NewestPublishedAsync<TEntity>(IQueryable<TEntity> set, CancellationToken ct)
        where TEntity : ContentEntity
    {
        var newest = await set.AsNoTracking()
            .Where(e => e.Status == ContentStatus.Published)
            .MaxAsync(e => (DateTime?)e.UpdatedAt, ct);
        return newest ?? FallbackLastMod;
    }
}
