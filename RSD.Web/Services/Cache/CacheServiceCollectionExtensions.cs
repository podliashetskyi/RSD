namespace RSD.Web.Services.Cache;

public static class CacheServiceCollectionExtensions
{
    public static IServiceCollection AddRsdCache(this IServiceCollection services, IConfiguration configuration)
    {
        var ttlSeconds = configuration.GetValue("OutputCache:DefaultTtlSeconds", 600);
        services.AddOutputCache(options =>
        {
            options.DefaultExpirationTimeSpan = TimeSpan.FromSeconds(ttlSeconds);
        });
        services.AddSingleton<IPublicPageCache, OutputCacheAdapter>();
        return services;
    }
}
