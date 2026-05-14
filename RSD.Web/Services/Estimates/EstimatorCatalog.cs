using RSD.Web.Data.Entities;

namespace RSD.Web.Services.Estimates;

public static class EstimatorCatalog
{
    public static string PlatformLabel(ProjectPlatform p) => p switch
    {
        ProjectPlatform.MobileApp => "Mobile App",
        ProjectPlatform.WebPlatform => "Web Platform",
        ProjectPlatform.DesktopApp => "Desktop App",
        _ => p.ToString(),
    };

    public static string DomainLabel(ProjectDomain d) => d switch
    {
        ProjectDomain.Ecommerce => "E-commerce",
        ProjectDomain.Healthcare => "Healthcare",
        ProjectDomain.Fintech => "Fintech",
        _ => d.ToString(),
    };

    public static string ComplexityLabel(ProjectComplexity c) => c switch
    {
        ProjectComplexity.SimpleMvp => "Simple (MVP)",
        ProjectComplexity.MediumProfessional => "Medium (Professional)",
        ProjectComplexity.ComplexEnterprise => "Complex (Enterprise)",
        _ => c.ToString(),
    };

    public static string ComplexityDescription(ProjectComplexity c) => c switch
    {
        ProjectComplexity.SimpleMvp => "Perfect for startups and testing ideas. Includes essential core features, a clean standard UI, and basic data management to get your product to market quickly.",
        ProjectComplexity.MediumProfessional => "A balanced solution for growing businesses. Includes custom UI/UX design, seamless third-party integrations (APIs), and enhanced performance optimization.",
        ProjectComplexity.ComplexEnterprise => "High-end systems built for scale. Includes high-load architecture, advanced multi-level security, complex data processing, and full-suite custom functionality.",
        _ => "",
    };

    public static string TimelineLabel(ProjectTimeline t) => t switch
    {
        ProjectTimeline.Standard => "Standard",
        ProjectTimeline.Accelerated => "Accelerated",
        ProjectTimeline.Urgent => "Urgent",
        _ => t.ToString(),
    };

    public static string TimelineWeeks(ProjectTimeline t) => t switch
    {
        ProjectTimeline.Standard => "8-12 weeks",
        ProjectTimeline.Accelerated => "4-8 weeks",
        ProjectTimeline.Urgent => "2-4 weeks",
        _ => "",
    };

    public static string SummaryChip(ProjectPlatform p, ProjectDomain d, ProjectComplexity c, ProjectTimeline t) =>
        $"{PlatformLabel(p)} · {DomainLabel(d)} · {ComplexityLabel(c)} · {TimelineLabel(t)}";
}
