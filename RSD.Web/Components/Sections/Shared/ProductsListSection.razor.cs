#pragma warning disable S1144, S4487, S2933
using Microsoft.AspNetCore.Components;

namespace RSD.Web.Components.Sections.Shared;

public partial class ProductsListSection
{
    [Parameter] public bool ShowHeader        { get; set; } = true;
    [Parameter] public bool ShowViewAllButton { get; set; } = true;

    private static readonly IReadOnlyList<ProductEntry> Products =
    [
        new ProductEntry(
            Name:           "NexaCRM",
            Subtitle:       "Next-Generation CRM",
            Price:          "from $49/mo",
            Description:    "Intelligent CRM system with AI assistant, sales automation, and real-time analytics.",
            BulletPoints:   ["AI Sales Forecasting", "Funnel Automation", "50+ Service Integrations", "Real-time Analytics"],
            ImageSrc:       "images/products/product-crm.png",
            TryForFreeHref: "/contact",
            LearnMoreHref:  "/products/nexacrm"),
        new ProductEntry(
            Name:           "NexaHR",
            Subtitle:       "HR Platform of the Future",
            Price:          "from $99/mo",
            Description:    "Comprehensive HR management: recruiting, onboarding, performance review, and team development.",
            BulletPoints:   ["AI Resume Screening", "Automated Onboarding", "360° Evaluation", "People Analytics"],
            ImageSrc:       "images/products/product-hr.png",
            TryForFreeHref: "/contact",
            LearnMoreHref:  "#"),
        new ProductEntry(
            Name:           "NexaAnalytics",
            Subtitle:       "Business Analytics for Everyone",
            Price:          "from $29/mo",
            Description:    "No-code platform for data visualization and dashboard building with any data source.",
            BulletPoints:   ["50+ Data Connectors", "AI Insights", "Custom Dashboards", "Real-time Reporting"],
            ImageSrc:       "images/products/product-analytics.png",
            TryForFreeHref: "/contact",
            LearnMoreHref:  "#"),
    ];

    private static string DirectionClass(int index) =>
        index % 2 == 0 ? "lg:flex-row" : "lg:flex-row-reverse";
}

public record ProductEntry(
    string Name,
    string Subtitle,
    string Price,
    string Description,
    IReadOnlyList<string> BulletPoints,
    string ImageSrc,
    string TryForFreeHref,
    string LearnMoreHref);
