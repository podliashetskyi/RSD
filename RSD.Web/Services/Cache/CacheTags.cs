using RSD.Web.Data.Entities;

namespace RSD.Web.Services.Cache;

public static class CacheTags
{
    public static string Entity<TEntity>(Guid id) where TEntity : ContentEntity =>
        $"entity:{TypeKey<TEntity>()}:{id:N}";

    public static string List<TEntity>() where TEntity : ContentEntity =>
        $"list:{TypeKey<TEntity>()}";

    private static string TypeKey<TEntity>() where TEntity : ContentEntity =>
        typeof(TEntity).Name.ToLowerInvariant();
}
