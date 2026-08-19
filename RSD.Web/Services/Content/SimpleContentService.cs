using Microsoft.EntityFrameworkCore;
using RSD.Web.Data;
using RSD.Web.Data.Entities;
using RSD.Web.Services.Cache;
using RSD.Web.Services.Common;
using RSD.Web.Services.Slugs;
using RSD.Web.Services.Storage;

namespace RSD.Web.Services.Content;

public abstract class SimpleContentService<TEntity>(
    IDbContextFactory<AppDbContext> DbFactory,
    ISlugger Slugger,
    IPublicPageCache Cache,
    IFileRefCountTracker RefCounts) : ISimpleContentService<TEntity> where TEntity : ContentEntity
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
        input = Normalize(input);
        var validation = Validate(input);
        if (!validation.Ok) return Result.Fail<Guid>(validation.Error);
        await using var db = await DbFactory.CreateDbContextAsync(ct);
        var slugResult = await ResolveSlugAsync(input.Slug, NaturalKeyOf(input), currentId: null, ct);
        if (!slugResult.Ok) return Result.Fail<Guid>(slugResult.Error);
        input.Slug = slugResult.Value!;
        ApplyTimestamps(input, isCreate: true);
        db.Set<TEntity>().Add(input);
        await db.SaveChangesAsync(ct);
        await RefCounts.ApplyDeltaAsync([], EntityPaths.OfAny(input), ct);
        await Cache.EvictListAsync<TEntity>(ct);
        return Result.Ok(input.Id);
    }

    public async Task<Result<Unit>> UpdateAsync(TEntity input, CancellationToken ct)
    {
        input = Normalize(input);
        var validation = Validate(input);
        if (!validation.Ok) return Result.Fail(validation.Error);
        await using var db = await DbFactory.CreateDbContextAsync(ct);
        var existing = await db.Set<TEntity>().FirstOrDefaultAsync(e => e.Id == input.Id, ct);
        if (existing is null) return Result.Fail("Entity not found.");
        var oldPaths = EntityPaths.OfAny(existing).ToList();
        var slugResult = await ResolveSlugAsync(input.Slug, NaturalKeyOf(input), input.Id, ct);
        if (!slugResult.Ok) return Result.Fail(slugResult.Error);
        input.Slug = slugResult.Value!;
        db.Entry(existing).CurrentValues.SetValues(input);
        ApplyTimestamps(existing, isCreate: false);
        await db.SaveChangesAsync(ct);
        await RefCounts.ApplyDeltaAsync(oldPaths, EntityPaths.OfAny(existing), ct);
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
        MutateAsync(id, e => e.IsDeleted = false, ct, ignoreFilters: true);

    public async Task<Result<Unit>> HardDeleteAsync(Guid id, CancellationToken ct)
    {
        await using var db = await DbFactory.CreateDbContextAsync(ct);
        var entity = await db.Set<TEntity>().IgnoreQueryFilters().FirstOrDefaultAsync(e => e.Id == id, ct);
        if (entity is null) return Result.Fail("Entity not found.");
        if (!entity.IsDeleted) return Result.Fail("Soft-delete the entity before purging it.");
        var oldPaths = EntityPaths.OfAny(entity).ToList();
        db.Set<TEntity>().Remove(entity);
        await db.SaveChangesAsync(ct);
        await RefCounts.ApplyDeltaAsync(oldPaths, [], ct);
        await Cache.EvictForAsync<TEntity>(id, ct);
        return Result.Ok();
    }

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

    /// <summary>Per-entity hook for validating user-editable fields before saving.</summary>
    protected virtual Result<Unit> Validate(TEntity entity) => Result.Ok();

    /// <summary>
    /// Per-entity hook applied before validation on create and update — the single place
    /// for invariants like HTML sanitization, so every writer (admin UI, seeders, MCP tools)
    /// goes through them. Default: pass-through.
    /// </summary>
    protected virtual TEntity Normalize(TEntity input) => input;

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

    private async Task<Result<string>> ResolveSlugAsync(string supplied, string fallback, Guid? currentId, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(supplied))
        {
            var generated = await Slugger.GenerateUniqueAsync<TEntity>(fallback, currentId, ct);
            return Result.Ok(generated);
        }
        var slugified = Slugger.Slugify(supplied);
        if (string.IsNullOrEmpty(slugified)) return Result.Fail<string>("Slug is required.");
        var available = await Slugger.IsAvailableAsync<TEntity>(slugified, currentId, ct);
        return available
            ? Result.Ok(slugified)
            : Result.Fail<string>($"The slug '{slugified}' is already in use. Choose a different one.");
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
