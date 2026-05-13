namespace RSD.Web.Services.Preview;

public static class PreviewServiceCollectionExtensions
{
    public static IServiceCollection AddRsdPreview(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<PreviewOptions>(configuration.GetSection(PreviewOptions.SectionName));
        services.AddSingleton<IPreviewTokenSigner, HmacPreviewTokenSigner>();
        return services;
    }
}
