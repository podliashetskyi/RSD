using RSD.Web.Data.Entities;

namespace RSD.Web.Services.Cache;

public interface IPublicPageCache
{
    Task EvictForAsync<TEntity>(Guid id, CancellationToken ct) where TEntity : ContentEntity;
    Task EvictListAsync<TEntity>(CancellationToken ct) where TEntity : ContentEntity;
    Task EvictAllAsync(CancellationToken ct);
}
