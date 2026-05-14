namespace RSD.Web.Services.Storage;

public static class StorageServiceCollectionExtensions
{
    public static IServiceCollection AddRsdStorage(this IServiceCollection services)
    {
        services.AddSingleton<IFileStorage, LocalDiskFileStorage>();
        services.AddScoped<IFileRefCountTracker, FileRefCountTracker>();
        services.AddScoped<IMediaService, MediaService>();
        services.AddScoped<RefCountAuditor>();
        return services;
    }
}
