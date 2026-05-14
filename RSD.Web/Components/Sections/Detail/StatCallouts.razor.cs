#pragma warning disable S1144, S4487, S2933
using Microsoft.AspNetCore.Components;
using RSD.Web.Data.Entities;

namespace RSD.Web.Components.Sections.Detail;

public partial class StatCallouts
{
    [Parameter, EditorRequired] public IReadOnlyList<MetricCallout> Items { get; set; } = [];
}

