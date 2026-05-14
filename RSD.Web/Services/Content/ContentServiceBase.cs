using Microsoft.EntityFrameworkCore;
using RSD.Web.Data;
using RSD.Web.Data.Entities;
using RSD.Web.Services.Cache;
using RSD.Web.Services.Common;
using RSD.Web.Services.Slugs;

namespace RSD.Web.Services.Content;

/// <summary>
/// Generic CRUD/status pipeline shared by BlogService, CaseService, ProductService and
/// ServiceService. Concrete services contribute mappings (TUpsert → entity, entity → DTOs)
/// and a natural-key hint for slug generation; everything else is shared.
/// </summary>
public abstract class ContentServiceBase<TEntity, TListItem, TDetail, TUpsert>(
    IDbContextFactory<AppDbContext> DbFactory,
    ISlugger Slugger,
    IPublicPageCache Cache) : IContentService<TListItem, TDetail, TUpsert>
    where TEntity : ContentEntity
{
    public async Task<IReadOnlyList<TListItem>> ListAsync(ContentQuery query, CancellationToken ct)
    {
        await using var db = await DbFactory.CreateDbContextAsync(ct);
        var page = Math.Max(query.Page, 1);
        var size = Math.Clamp(query.PageSize, 1, 200);
        var q = db.Set<TEntity>().AsNoTracking();
        if (query.IncludeDeleted) q = q.IgnoreQueryFilters();
        q = ApplyStatusFilter(q, query.Status);
        q = ApplySearchFilter(q, query.Search);
        q = ApplyOrdering(q);
        var rows = await q.Skip((page - 1) * size).Take(size).ToListAsync(ct);
        return [.. rows.Select(ToListItem)];
    }

    public async Task<TDetail?> GetByIdAsync(Guid id, CancellationToken ct)
    {
        await using var db = await DbFactory.CreateDbContextAsync(ct);
        var entity = await db.Set<TEntity>().AsNoTracking().FirstOrDefaultAsync(e => e.Id == id, ct);
        return entity is null ? default : ToDetail(entity);
    }

    public async Task<TDetail?> GetBySlugAsync(string slug, bool includeDrafts, CancellationToken ct)
    {
        await using var db = await DbFactory.CreateDbContextAsync(ct);
        var q = db.Set<TEntity>().AsNoTracking().Where(e => e.Slug == slug);
        if (!includeDrafts) q = q.Where(e => e.Status == ContentStatus.Published);
        var entity = await q.FirstOrDefaultAsync(ct);
        return entity is null ? default : ToDetail(entity);
    }

    public async Task<Result<Guid>> CreateAsync(TUpsert input, CancellationToken ct)
    {
        await using var db = await DbFactory.CreateDbContextAsync(ct);
        var entity = NewEntityFrom(input);
        entity.Slug = await EnsureSlugAsync(entity.Slug, NaturalKeyOf(input), currentId: null, ct);
        db.Set<TEntity>().Add(entity);
        await db.SaveChangesAsync(ct);
        await Cache.EvictListAsync<TEntity>(ct);
        return Result.Ok(entity.Id);
    }

    public async Task<Result<Unit>> UpdateAsync(Guid id, TUpsert input, CancellationToken ct)
    {
        await using var db = await DbFactory.CreateDbContextAsync(ct);
        var existing = await db.Set<TEntity>().FirstOrDefaultAsync(e => e.Id == id, ct);
        if (existing is null) return Result.Fail("Entity not found.");
        var desiredSlug = SlugOf(input);
        existing.Slug = await EnsureSlugAsync(desiredSlug, NaturalKeyOf(input), id, ct);
        ApplyUpdate(existing, input);
        existing.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);
        await Cache.EvictForAsync<TEntity>(id, ct);
        return Result.Ok();
    }

    public Task<Result<Unit>> PublishAsync(Guid id, CancellationToken ct) =>
        SetStatusAsync(id, ContentStatus.Published, ct);

    public Task<Result<Unit>> UnpublishAsync(Guid id, CancellationToken ct) =>
        SetStatusAsync(id, ContentStatus.Draft, ct);

    public Task<Result<Unit>> ArchiveAsync(Guid id, CancellationToken ct) =>
        SetStatusAsync(id, ContentStatus.Archived, ct);

    public Task<Result<Unit>> SoftDeleteAsync(Guid id, CancellationToken ct) =>
        MutateAsync(id, e => e.IsDeleted = true, ct);

    public Task<Result<Unit>> RestoreAsync(Guid id, CancellationToken ct) =>
        MutateAsync(id, e =>
        {
            e.IsDeleted = false;
            e.Status = ContentStatus.Draft;
        }, ct, ignoreFilters: true);

    public async Task<Result<Unit>> HardDeleteAsync(Guid id, CancellationToken ct)
    {
        await using var db = await DbFactory.CreateDbContextAsync(ct);
        var entity = await db.Set<TEntity>().IgnoreQueryFilters().FirstOrDefaultAsync(e => e.Id == id, ct);
        if (entity is null) return Result.Fail("Entity not found.");
        if (!entity.IsDeleted) return Result.Fail("Soft-delete the entity before purging it.");
        db.Set<TEntity>().Remove(entity);
        await db.SaveChangesAsync(ct);
        await Cache.EvictForAsync<TEntity>(id, ct);
        return Result.Ok();
    }

    protected abstract TEntity NewEntityFrom(TUpsert input);
    protected abstract void ApplyUpdate(TEntity entity, TUpsert input);
    protected abstract TListItem ToListItem(TEntity entity);
    protected abstract TDetail ToDetail(TEntity entity);
    protected abstract string NaturalKeyOf(TUpsert input);
    protected abstract string SlugOf(TUpsert input);

    protected virtual IQueryable<TEntity> ApplyOrdering(IQueryable<TEntity> q) =>
        q.OrderByDescending(e => e.UpdatedAt);

    private static IQueryable<TEntity> ApplyStatusFilter(IQueryable<TEntity> q, ContentStatus? status) =>
        status is { } s ? q.Where(e => e.Status == s) : q;

    private static IQueryable<TEntity> ApplySearchFilter(IQueryable<TEntity> q, string search) =>
        string.IsNullOrWhiteSpace(search) ? q : q.Where(e => EF.Functions.ILike(e.Slug, $"%{search}%"));

    private async Task<string> EnsureSlugAsync(string supplied, string fallback, Guid? currentId, CancellationToken ct)
    {
        var seed = string.IsNullOrWhiteSpace(supplied) ? fallback : supplied;
        return await Slugger.GenerateUniqueAsync<TEntity>(seed, currentId, ct);
    }

    private Task<Result<Unit>> SetStatusAsync(Guid id, ContentStatus status, CancellationToken ct) =>
        MutateAsync(id, e =>
        {
            e.Status = status;
            if (status == ContentStatus.Published && e.PublishedAt is null) e.PublishedAt = DateTime.UtcNow;
        }, ct);

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
