namespace RSD.Web.Services.Slugs;

public static class SlugsServiceCollectionExtensions
{
    public static IServiceCollection AddRsdSlugs(this IServiceCollection services)
    {
        services.AddScoped<ISlugger, Slugger>();
        return services;
    }
}
