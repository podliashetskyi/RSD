namespace RSD.Web.Services.Storage;

public static class StorageServiceCollectionExtensions
{
    public static IServiceCollection AddRsdStorage(this IServiceCollection services)
    {
        services.AddSingleton<IFileStorage, LocalDiskFileStorage>();
        return services;
    }
}
