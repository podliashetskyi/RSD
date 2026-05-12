#pragma warning disable S1144, S4487, S2933
using Microsoft.AspNetCore.Components;
using RSD.Web.Components.Sections.Article;

namespace RSD.Web.Components.Pages;

public partial class ServiceDetail(NavigationManager Nav)
{
    [Parameter] public string Slug { get; set; } = "";

    private static readonly IReadOnlyList<TocEntry> TocItems =
    [
        new("innovation-in-cloud",   "Innovation in Cloud"),
        new("key-features",          "Key Features"),
        new("our-recommendation",    "Our Recommendation"),
        new("scaling-effectively",   "Scaling Effectively"),
        new("implementation-roadmap","Implementation Roadmap"),
        new("tools-we-use",          "Tools We Use"),
    ];

    private static readonly IReadOnlyList<SubsectionItem> KeyFeaturesUX =
    [
        new("High Availability:", "Automated failover systems to ensure zero downtime."),
        new("Cost Efficiency:",   "Smart resource allocation to reduce cloud infrastructure spending."),
        new("Security First:",    "Multi-layer encryption and compliance with global standards."),
        new("Rapid Deployment:",  "CI/CD pipelines that cut time-to-market by 40%."),
    ];

    private static readonly IReadOnlyList<SubsectionItem> KeyFeaturesDesign =
    [
        new("User Research:",  "Deep dive into user behavior and market needs."),
        new("Prototyping:",    "Creating interactive models to test ideas early."),
        new("Design Systems:", "Building scalable and consistent visual language."),
        new("A/B Testing:",    "Data-driven design optimization for better conversions."),
    ];

    private static readonly IReadOnlyList<SubsectionItem> ScalingEffectively =
    [
        new("Prioritize Microservices:",
            "We recommend moving to a microservices architecture to avoid a \"single point of failure\" of the entire system."),
        new("Automate Deployment:",
            "Implementing CI/CD pipelines allows you to reduce time-to-market by 40%."),
        new("Optimize Cloud Costs:",
            "Constant monitoring of resources allows you to save up to 30% of the budget for the infrastructure being contained."),
        new("Security by Design:",
            "Build the system with safety in mind at every stage, ensuring stability during peak loads."),
    ];

    private static readonly IReadOnlyList<SubsectionItem> ImplementationRoadmap =
    [
        new("High Availability:", "Automated failover systems to ensure zero downtime."),
        new("Cost Efficiency:",   "Smart resource allocation to reduce cloud infrastructure spending."),
        new("Security First:",    "Multi-layer encryption and compliance with global standards."),
        new("Rapid Deployment:",  "CI/CD pipelines that cut time-to-market by 40%."),
    ];

    private static readonly IReadOnlyList<StatRowItem> RecommendationStats =
    [
        new("99.9%", "Target Uptime."),
        new("40%",   "Potential Cost Reduction."),
        new("200+",  "Optimization Checkpoints."),
        new("4M+",   "Users Supported."),
    ];

    private static readonly IReadOnlyList<GalleryImage> GalleryImages =
    [
        new("images/services/cloud-solutions/gallery-1-big.png", "Mobile dashboard mockups"),
        new("images/services/cloud-solutions/gallery-1a.png",    "Globe network visualization"),
        new("images/services/cloud-solutions/gallery-1b.png",    "Connected nodes visualization"),
        new("images/services/cloud-solutions/gallery-2a.png",    "Cloud network globe"),
        new("images/services/cloud-solutions/gallery-2b.png",    "Server room"),
        new("images/services/cloud-solutions/gallery-2-big.png", "Server infrastructure"),
    ];

    protected override void OnParametersSet()
    {
        if (!string.Equals(Slug, "cloud-solutions", StringComparison.OrdinalIgnoreCase))
        {
            Nav.NavigateTo("/services", replace: true);
        }
    }
}
