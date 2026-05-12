#pragma warning disable S1144, S4487, S2933
using Microsoft.AspNetCore.Components;
using RSD.Web.Components.Sections.Detail;

namespace RSD.Web.Components.Pages;

public partial class ProductDetail(NavigationManager Nav)
{
    [Parameter] public string Slug { get; set; } = "";

    protected override void OnParametersSet()
    {
        if (!string.Equals(Slug, "nexacrm", StringComparison.OrdinalIgnoreCase))
        {
            Nav.NavigateTo("/products", replace: true);
        }
    }

    private static readonly IReadOnlyList<BadgePill> ProductBadges =
    [
        new("Available", "bg-success-100", "text-success-600"),
        new("from $49/mo"),
    ];

    private static readonly IReadOnlyList<string> Features =
    [
        "AI Sales Forecasting",
        "50+ Service Integrations",
        "Custom Workflows",
        "React, Vue, Angular",
        "Node.js, Python, Go",
        "Mobile App",
    ];

    private static readonly IReadOnlyList<MetaItem> ChallengeMeta =
    [
        new("Product",   "Digital Service / App"),
        new("Industry",  "Healthcare"),
        new("Timeframe", "2011 – Present"),
    ];

    private static readonly IReadOnlyList<ChallengeHurdle> Hurdles =
    [
        new("Accessibility & Inclusion",
            "Designing an interface that remains fully functional for users with limited motor skills, ensuring they can navigate the app with minimal effort."),
        new("Real-Time Reliability",
            "For a person using a communication aid, the system is their \"voice.\" Any lag or system failure results in an immediate loss of ability to interact with the world, making 99.9% uptime non-negotiable."),
        new("Natural Interaction",
            "Moving beyond simple pre-recorded phrases to integrate advanced Word Prediction (NLP) that learns from the user’s behavior to speed up the communication process."),
        new("Platform Stability",
            "Building a robust architecture capable of supporting continuous updates and new language integrations without disrupting the existing user base."),
    ];

    private static readonly IReadOnlyList<string> Results =
    [
        "50,000+ active patients",
        "99.9% uptime achieved",
        "30+ IoT device integrations",
        "HIPAA certified",
    ];

    private static readonly IReadOnlyList<MetricCallout> Metrics =
    [
        new("Increase Sales by 40%",
            "Our AI-powered insights help identify the best opportunities and optimal timing for outreach."),
        new("Save 10+ Hours Weekly",
            "Automate data entry, follow-ups, and reporting to focus on what matters most."),
        new("360° Customer View",
            "All customer interactions, history, and preferences in one unified dashboard."),
    ];

    private static readonly IReadOnlyList<string> TechPills =
    [
        "Flutter", "Dart", "AWS Lambda", "DynamoDB", "WebRTC", "IoT Core", "FHIR API",
    ];
}
