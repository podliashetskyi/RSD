#pragma warning disable S1144, S4487, S2933
using Microsoft.AspNetCore.Components;

namespace RSD.Web.Components.Sections.Detail;

public partial class IdentityBar
{
    [Parameter, EditorRequired] public string BackHref { get; set; } = "";
    [Parameter, EditorRequired] public string Title { get; set; } = "";
    [Parameter, EditorRequired] public string Subtitle { get; set; } = "";
    [Parameter] public string RightLabel { get; set; } = "";
    [Parameter, EditorRequired] public string CtaText { get; set; } = "";
    [Parameter] public string CtaHref { get; set; } = "#";
}
