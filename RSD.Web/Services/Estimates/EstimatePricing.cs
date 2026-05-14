using RSD.Web.Data.Entities;

namespace RSD.Web.Services.Estimates;

public static class EstimatePricing
{
    private const decimal RangeFactor = 1.5m;

    public static (decimal Min, decimal Max) Compute(
        ProjectPlatform platform,
        ProjectDomain domain,
        ProjectComplexity complexity,
        ProjectTimeline timeline)
    {
        var raw = BasePrice(platform) * DomainMultiplier(domain) * ComplexityMultiplier(complexity) * TimelineMultiplier(timeline);
        var min = RoundTo500(raw);
        var max = RoundTo500(raw * RangeFactor);
        return (min, max);
    }

    public static decimal BasePrice(ProjectPlatform platform) => platform switch
    {
        ProjectPlatform.MobileApp => 25_000m,
        ProjectPlatform.WebPlatform => 15_000m,
        ProjectPlatform.DesktopApp => 20_000m,
        _ => 0m,
    };

    public static decimal DomainMultiplier(ProjectDomain domain) => domain switch
    {
        ProjectDomain.Ecommerce => 1.4m,
        ProjectDomain.Healthcare => 1.6m,
        ProjectDomain.Fintech => 1.8m,
        _ => 1m,
    };

    public static decimal ComplexityMultiplier(ProjectComplexity complexity) => complexity switch
    {
        ProjectComplexity.SimpleMvp => 1.0m,
        ProjectComplexity.MediumProfessional => 1.4m,
        ProjectComplexity.ComplexEnterprise => 1.8m,
        _ => 1m,
    };

    public static decimal TimelineMultiplier(ProjectTimeline timeline) => timeline switch
    {
        ProjectTimeline.Standard => 1.0m,
        ProjectTimeline.Accelerated => 1.2m,
        ProjectTimeline.Urgent => 1.5m,
        _ => 1m,
    };

    private static decimal RoundTo500(decimal value) =>
        Math.Round(value / 500m, MidpointRounding.AwayFromZero) * 500m;
}
