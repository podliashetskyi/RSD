using Microsoft.EntityFrameworkCore;
using RSD.Web.Data;
using RSD.Web.Data.Entities;
using RSD.Web.Services.Common;

namespace RSD.Web.Services.Storage;

public sealed class MediaService(
    IDbContextFactory<AppDbContext> DbFactory,
    IFileStorage Storage) : IMediaService
{
    public async Task<IReadOnlyList<MediaListItem>> ListAsync(MediaQuery query, CancellationToken ct)
    {
        await using var db = await DbFactory.CreateDbContextAsync(ct);
        var page = Math.Max(query.Page, 1);
        var size = Math.Clamp(query.PageSize, 1, 200);
        var q = db.UploadedFiles.AsNoTracking().AsQueryable();
        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var pattern = $"%{query.Search}%";
            q = q.Where(f => EF.Functions.ILike(f.OriginalName, pattern) || EF.Functions.ILike(f.ContentType, pattern));
        }
        return await q.OrderByDescending(f => f.UploadedAt)
            .Skip((page - 1) * size).Take(size)
            .Select(f => new MediaListItem(f.Id, f.Path, f.OriginalName, f.ContentType, f.Bytes, f.UploadedAt, f.RefCount))
            .ToListAsync(ct);
    }

    public async Task<UploadedFile?> GetByIdAsync(Guid id, CancellationToken ct)
    {
        await using var db = await DbFactory.CreateDbContextAsync(ct);
        return await db.UploadedFiles.AsNoTracking().FirstOrDefaultAsync(f => f.Id == id, ct);
    }

    public async Task<IReadOnlyList<MediaReference>> UsedByAsync(string path, CancellationToken ct)
    {
        await using var db = await DbFactory.CreateDbContextAsync(ct);
        var refs = new List<MediaReference>();
        await AddRefsAsync(refs, db.BlogPosts, e => e.CoverImagePath == path, "blog", "Blog post", e => new(e.Id, e.Title, e.Slug), ct);
        await AddRefsAsync(refs, db.Cases, e => e.CoverImagePath == path, "cases", "Case", e => new(e.Id, e.Name, e.Slug), ct);
        await AddRefsAsync(refs, db.Products, e => e.CoverImagePath == path, "products", "Product", e => new(e.Id, e.Name, e.Slug), ct);
        await AddRefsAsync(refs, db.Services, e => e.CoverImagePath == path, "services", "Service", e => new(e.Id, e.Title, e.Slug), ct);
        await AddRefsAsync(refs, db.Testimonials, e => e.AvatarPath == path, "testimonials", "Testimonial", e => new(e.Id, e.Title, e.Slug), ct);
        await AddRefsAsync(refs, db.TeamMembers, e => e.AvatarPath == path, "team", "Team member", e => new(e.Id, e.Name, e.Slug), ct);
        await AddRefsAsync(refs, db.Partners, e => e.PhotoPath == path, "partners", "Partner", e => new(e.Id, e.Name, e.Slug), ct);
        await AddRefsAsync(refs, db.Values, e => e.IconPath == path, "values", "Value", e => new(e.Id, e.Title, e.Slug), ct);
        await AddRefsAsync(refs, db.TechStackItems, e => e.LogoPath == path, "tech", "Tech stack item", e => new(e.Id, e.Label, e.Slug), ct);
        await AddRefsAsync(refs, db.MessengerLinks, e => e.LargeIconPath == path || e.SmallIconPath == path, "messenger-links", "Messenger link", e => new(e.Id, e.Label, e.Slug), ct);
        await AddRefsAsync(refs, db.SocialLinks, e => e.IconPath == path, "social-links", "Social link", e => new(e.Id, e.Label, e.Slug), ct);
        return refs;
    }

    public async Task<Result<Unit>> HardDeleteAsync(Guid id, CancellationToken ct)
    {
        await using var db = await DbFactory.CreateDbContextAsync(ct);
        var file = await db.UploadedFiles.FirstOrDefaultAsync(f => f.Id == id, ct);
        if (file is null) return Result.Fail("File not found.");
        if (file.RefCount > 0) return Result.Fail($"File is still referenced by {file.RefCount} entit{(file.RefCount == 1 ? "y" : "ies")}.");
        var paths = new List<string> { file.Path };
        paths.AddRange(file.Variants.Select(v => v.Path));
        db.UploadedFiles.Remove(file);
        await db.SaveChangesAsync(ct);
        foreach (var p in paths)
        {
            try { await Storage.DeleteAsync(p, ct); }
            catch (IOException) { /* file already gone; the DB row is what mattered */ }
        }
        return Result.Ok();
    }

    private static async Task AddRefsAsync<TEntity>(
        List<MediaReference> refs,
        IQueryable<TEntity> set,
        System.Linq.Expressions.Expression<Func<TEntity, bool>> predicate,
        string entityKey,
        string entityLabel,
        System.Linq.Expressions.Expression<Func<TEntity, RefShape>> project,
        CancellationToken ct) where TEntity : ContentEntity
    {
        var rows = await set.IgnoreQueryFilters().Where(predicate).Select(project).ToListAsync(ct);
        foreach (var r in rows) refs.Add(new MediaReference(entityKey, entityLabel, r.Id, r.Title, r.Slug));
    }

    private readonly record struct RefShape(Guid Id, string Title, string Slug);
}
