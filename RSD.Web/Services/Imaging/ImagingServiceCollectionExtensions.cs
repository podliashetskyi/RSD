namespace RSD.Web.Services.Imaging;

public static class ImagingServiceCollectionExtensions
{
    public static IServiceCollection AddRsdImaging(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<ImagingOptions>(configuration.GetSection(ImagingOptions.SectionName));
        services.AddSingleton<SvgSanitizer>();
        services.AddSingleton<IImageProcessor, ImageSharpProcessor>();
        return services;
    }
}
