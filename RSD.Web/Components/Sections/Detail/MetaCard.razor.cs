#pragma warning disable S1144, S4487, S2933
using Microsoft.AspNetCore.Components;

namespace RSD.Web.Components.Sections.Detail;

public partial class MetaCard
{
    [Parameter] public IReadOnlyList<BadgePill> Badges { get; set; } = [];
    [Parameter, EditorRequired] public string Title { get; set; } = "";
    [Parameter] public string Subtitle { get; set; } = "";
    [Parameter, EditorRequired] public string Description { get; set; } = "";
    [Parameter] public IReadOnlyList<string> Tags { get; set; } = [];
    [Parameter] public string PrimaryText { get; set; } = "";
    [Parameter] public string PrimaryTextMobile { get; set; } = "";
    [Parameter] public string PrimaryHref { get; set; } = "#";
    [Parameter] public string SecondaryText { get; set; } = "";
    [Parameter] public string SecondaryHref { get; set; } = "#";
}

public record BadgePill(string Text, string BgClass = "", string TextClass = "");
