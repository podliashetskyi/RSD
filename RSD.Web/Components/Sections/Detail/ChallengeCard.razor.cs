#pragma warning disable S1144, S4487, S2933
using Microsoft.AspNetCore.Components;
using RSD.Web.Data.Entities;

namespace RSD.Web.Components.Sections.Detail;

public partial class ChallengeCard
{
    [Parameter] public string Heading { get; set; } = "The Challenge";
    [Parameter, EditorRequired] public IReadOnlyList<MetaItem> MetaItems { get; set; } = [];
    [Parameter, EditorRequired] public string Body { get; set; } = "";
    [Parameter, EditorRequired] public IReadOnlyList<ChallengeHurdle> Hurdles { get; set; } = [];

    // The meta strip only needs its divider when hurdles render beneath it.
    private string MetaRowClass => Hurdles.Count > 0
        ? "border-b border-line flex flex-col lg:flex-row gap-2 lg:gap-8 pb-3 px-3"
        : "flex flex-col lg:flex-row gap-2 lg:gap-8 px-3";
}

