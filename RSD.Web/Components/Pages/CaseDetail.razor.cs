#pragma warning disable S1144, S4487, S2933
using Microsoft.AspNetCore.Components;
using RSD.Web.Components.Sections.Detail;

namespace RSD.Web.Components.Pages;

public partial class CaseDetail(NavigationManager Nav)
{
    [Parameter] public string Slug { get; set; } = "";

    private static readonly IReadOnlyList<BadgePill> MetaBadges =
    [
        new("Healthcare",     "bg-cyan-100", "text-cyan-700"),
        new("23 months",      "bg-cyan-100", "text-cyan-700"),
        new("10 specialists", "bg-cyan-100", "text-cyan-700"),
    ];

    private static readonly IReadOnlyList<string> MetaTags =
    [
        "Flutter", "AWS", "IoT", "HIPAA",
    ];

    private static readonly IReadOnlyList<string> Results =
    [
        "50,000+ active patients",
        "30+ IoT device integrations",
        "99.9% uptime achieved",
        "HIPAA certified",
    ];

    private static readonly IReadOnlyList<string> TechPills =
    [
        "Flutter", "Dart", "AWS Lambda", "DynamoDB", "WebRTC", "IoT Core", "FHIR API",
    ];

    protected override void OnParametersSet()
    {
        if (!string.Equals(Slug, "healthcare-plus", StringComparison.OrdinalIgnoreCase))
        {
            Nav.NavigateTo("/cases", replace: true);
        }
    }
}
