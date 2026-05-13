using Microsoft.AspNetCore.OutputCaching;
using RSD.Web.Data.Entities;

namespace RSD.Web.Services.Cache;

public sealed class OutputCacheAdapter(IOutputCacheStore Store) : IPublicPageCache
{
    private const string AllTag = "all";

    public async Task EvictForAsync<TEntity>(Guid id, CancellationToken ct) where TEntity : ContentEntity
    {
        await Store.EvictByTagAsync(CacheTags.Entity<TEntity>(id), ct);
        await Store.EvictByTagAsync(CacheTags.List<TEntity>(), ct);
    }

    public Task EvictListAsync<TEntity>(CancellationToken ct) where TEntity : ContentEntity =>
        Store.EvictByTagAsync(CacheTags.List<TEntity>(), ct).AsTask();

    public Task EvictAllAsync(CancellationToken ct) =>
        Store.EvictByTagAsync(AllTag, ct).AsTask();
}
