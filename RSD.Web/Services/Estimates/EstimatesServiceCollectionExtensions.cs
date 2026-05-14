namespace RSD.Web.Services.Estimates;

public static class EstimatesServiceCollectionExtensions
{
    public static IServiceCollection AddRsdEstimates(this IServiceCollection services)
    {
        services.AddScoped<IProjectEstimateService, ProjectEstimateService>();
        return services;
    }
}
