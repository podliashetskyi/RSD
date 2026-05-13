using Microsoft.EntityFrameworkCore;
using RSD.Web.Data;
using RSD.Web.Data.Entities;
using RSD.Web.Services.Cache;
using RSD.Web.Services.Common;
using RSD.Web.Services.Slugs;

namespace RSD.Web.Services.Content;

public abstract class SimpleContentService<TEntity>(
    IDbContextFactory<AppDbContext> DbFactory,
    ISlugger Slugger,
    IPublicPageCache Cache) : ISimpleContentService<TEntity> where TEntity : ContentEntity
{
    public async Task<IReadOnlyList<TEntity>> ListAsync(ContentQuery query, CancellationToken ct)
    {
        await using var db = await DbFactory.CreateDbContextAsync(ct);
        var page = Math.Max(query.Page, 1);
        var size = Math.Clamp(query.PageSize, 1, 200);
        var q = BaseQuery(db, query);
        q = ApplyStatusFilter(q, query.Status);
        q = ApplySearchFilter(q, query.Search);
        return await q.OrderBy(e => e.Slug)
                      .Skip((page - 1) * size)
                      .Take(size)
                      .ToListAsync(ct);
    }

    public async Task<TEntity?> GetByIdAsync(Guid id, CancellationToken ct)
    {
        await using var db = await DbFactory.CreateDbContextAsync(ct);
        return await db.Set<TEntity>().AsNoTracking().FirstOrDefaultAsync(e => e.Id == id, ct);
    }

    public async Task<Result<Guid>> CreateAsync(TEntity input, CancellationToken ct)
    {
        await using var db = await DbFactory.CreateDbContextAsync(ct);
        input.Slug = await EnsureSlugAsync(input.Slug, NaturalKeyOf(input), currentId: null, ct);
        ApplyTimestamps(input, isCreate: true);
        db.Set<TEntity>().Add(input);
        await db.SaveChangesAsync(ct);
        await Cache.EvictListAsync<TEntity>(ct);
        return Result.Ok(input.Id);
    }

    public async Task<Result<Unit>> UpdateAsync(TEntity input, CancellationToken ct)
    {
        await using var db = await DbFactory.CreateDbContextAsync(ct);
        var existing = await db.Set<TEntity>().FirstOrDefaultAsync(e => e.Id == input.Id, ct);
        if (existing is null) return Result.Fail("Entity not found.");
        input.Slug = await EnsureSlugAsync(input.Slug, NaturalKeyOf(input), input.Id, ct);
        db.Entry(existing).CurrentValues.SetValues(input);
        ApplyTimestamps(existing, isCreate: false);
        await db.SaveChangesAsync(ct);
        await Cache.EvictForAsync<TEntity>(input.Id, ct);
        return Result.Ok();
    }

    public Task<Result<Unit>> SetStatusAsync(Guid id, ContentStatus status, CancellationToken ct) =>
        MutateAsync(id, e =>
        {
            e.Status = status;
            e.PublishedAt = status == ContentStatus.Published ? DateTime.UtcNow : e.PublishedAt;
        }, ct);

    public Task<Result<Unit>> SoftDeleteAsync(Guid id, CancellationToken ct) =>
        MutateAsync(id, e => e.IsDeleted = true, ct);

    public Task<Result<Unit>> RestoreAsync(Guid id, CancellationToken ct) =>
        MutateAsync(id, e =>
        {
            e.IsDeleted = false;
            e.Status = ContentStatus.Draft;
        }, ct, ignoreFilters: true);

    public async Task<Result<Unit>> BulkReorderAsync(IReadOnlyList<ReorderEntry> ordered, CancellationToken ct)
    {
        if (ordered.Count == 0) return Result.Ok();
        await using var db = await DbFactory.CreateDbContextAsync(ct);
        var ids = ordered.Select(o => o.Id).ToList();
        var entities = await db.Set<TEntity>().Where(e => ids.Contains(e.Id)).ToListAsync(ct);
        if (!CanReorder(entities)) return Result.Fail("This entity type does not support reordering.");
        var orderMap = ordered.ToDictionary(o => o.Id, o => o.DisplayOrder);
        ApplyOrderings(entities, orderMap);
        await db.SaveChangesAsync(ct);
        await Cache.EvictListAsync<TEntity>(ct);
        return Result.Ok();
    }

    private static bool CanReorder(IReadOnlyList<TEntity> entities) =>
        entities.Count == 0 || entities[0] is IHasDisplayOrder;

    private static void ApplyOrderings(IReadOnlyList<TEntity> entities, Dictionary<Guid, int> orderMap)
    {
        foreach (var e in entities)
        {
            if (e is IHasDisplayOrder ordered) ordered.DisplayOrder = orderMap[e.Id];
            e.UpdatedAt = DateTime.UtcNow;
        }
    }

    /// <summary>Per-entity hook for picking a natural source for the slug when one isn't supplied.</summary>
    protected abstract string NaturalKeyOf(TEntity entity);

    private static IQueryable<TEntity> BaseQuery(AppDbContext db, ContentQuery query)
    {
        var q = db.Set<TEntity>().AsNoTracking();
        if (query.IncludeDeleted) q = q.IgnoreQueryFilters();
        return q;
    }

    private static IQueryable<TEntity> ApplyStatusFilter(IQueryable<TEntity> q, ContentStatus? status) =>
        status is { } s ? q.Where(e => e.Status == s) : q;

    private static IQueryable<TEntity> ApplySearchFilter(IQueryable<TEntity> q, string search) =>
        string.IsNullOrWhiteSpace(search) ? q : q.Where(e => EF.Functions.ILike(e.Slug, $"%{search}%"));

    private async Task<string> EnsureSlugAsync(string supplied, string fallback, Guid? currentId, CancellationToken ct)
    {
        var seed = string.IsNullOrWhiteSpace(supplied) ? fallback : supplied;
        return await Slugger.GenerateUniqueAsync<TEntity>(seed, currentId, ct);
    }

    private static void ApplyTimestamps(TEntity entity, bool isCreate)
    {
        var now = DateTime.UtcNow;
        if (isCreate) return;
        entity.UpdatedAt = now;
    }

    private async Task<Result<Unit>> MutateAsync(Guid id, Action<TEntity> mutate, CancellationToken ct, bool ignoreFilters = false)
    {
        await using var db = await DbFactory.CreateDbContextAsync(ct);
        var set = db.Set<TEntity>();
        var query = ignoreFilters ? set.IgnoreQueryFilters() : set.AsQueryable();
        var entity = await query.FirstOrDefaultAsync(e => e.Id == id, ct);
        if (entity is null) return Result.Fail("Entity not found.");
        mutate(entity);
        entity.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);
        await Cache.EvictForAsync<TEntity>(id, ct);
        return Result.Ok();
    }
}
