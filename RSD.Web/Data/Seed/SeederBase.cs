using Microsoft.EntityFrameworkCore;
using RSD.Web.Data.Entities;
using RSD.Web.Services.Slugs;

namespace RSD.Web.Data.Seed;

public abstract class SeederBase<TEntity>(AppDbContext Db, ISlugger Slugger) : ISeeder
    where TEntity : ContentEntity
{
    public async Task SeedAsync(CancellationToken ct)
    {
        if (await Db.Set<TEntity>().AsNoTracking().AnyAsync(ct)) return;
        var items = await BuildAsync(ct);
        await AssignUniqueSlugsAsync(items, ct);
        Db.Set<TEntity>().AddRange(items);
        await Db.SaveChangesAsync(ct);
    }

    protected abstract Task<IReadOnlyList<TEntity>> BuildAsync(CancellationToken ct);

    private async Task AssignUniqueSlugsAsync(IReadOnlyList<TEntity> items, CancellationToken ct)
    {
        var used = new HashSet<string>(StringComparer.Ordinal);
        foreach (var item in items) item.Slug = await NextSlugAsync(item.Slug, used, ct);
    }

    private async Task<string> NextSlugAsync(string seed, HashSet<string> used, CancellationToken ct)
    {
        var candidate = await Slugger.GenerateUniqueAsync<TEntity>(seed, currentId: null, ct);
        var unique = AppendSuffixUntilFree(candidate, used);
        used.Add(unique);
        return unique;
    }

    private static string AppendSuffixUntilFree(string baseSlug, HashSet<string> used)
    {
        if (!used.Contains(baseSlug)) return baseSlug;
        for (var n = 2; n <= 1000; n++)
        {
            var candidate = $"{baseSlug}-{n}";
            if (!used.Contains(candidate)) return candidate;
        }
        throw new InvalidOperationException($"Could not allocate an in-batch slug for '{baseSlug}'.");
    }
}
