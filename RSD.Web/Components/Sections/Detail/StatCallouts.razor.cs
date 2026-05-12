#pragma warning disable S1144, S4487, S2933
using Microsoft.AspNetCore.Components;

namespace RSD.Web.Components.Sections.Detail;

public partial class StatCallouts
{
    [Parameter, EditorRequired] public IReadOnlyList<MetricCallout> Items { get; set; } = [];
}

public record MetricCallout(string Headline, string Description);
