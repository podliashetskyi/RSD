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
}

