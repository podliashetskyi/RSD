namespace RSD.Web.Services.Seo;

public static class SeoServiceCollectionExtensions
{
    public static IServiceCollection AddRsdSeo(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<SeoOptions>(configuration.GetSection(SeoOptions.SectionName));
        services.AddScoped<ISitemapBuilder, SitemapBuilder>();
        services.AddSingleton<IRobotsTxtProvider, RobotsTxtProvider>();
        return services;
    }
}
