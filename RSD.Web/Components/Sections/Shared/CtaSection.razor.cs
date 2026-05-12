#pragma warning disable S1144, S4487, S2933
using Microsoft.AspNetCore.Components;

namespace RSD.Web.Components.Sections.Shared;

public partial class CtaSection
{
    [Parameter] public string Heading { get; set; } = "Ready to Start Your Project?";
    [Parameter] public string Description { get; set; } = "Get a free consultation and project estimate from our experts. We'll help bring your idea to life.";

    [Parameter] public string PrimaryButtonText { get; set; } = "Calculate Project Cost";
    [Parameter] public string PrimaryButtonHref { get; set; } = "/contact";
    [Parameter] public string SecondaryButtonText { get; set; } = "Book a Call";
    [Parameter] public string SecondaryButtonHref { get; set; } = "/contact";
}
