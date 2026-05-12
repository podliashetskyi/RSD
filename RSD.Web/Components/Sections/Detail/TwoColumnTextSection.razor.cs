#pragma warning disable S1144, S4487, S2933
using Microsoft.AspNetCore.Components;

namespace RSD.Web.Components.Sections.Detail;

public partial class TwoColumnTextSection
{
    [Parameter, EditorRequired] public string LeftHeading { get; set; } = "";
    [Parameter, EditorRequired] public string LeftBody { get; set; } = "";
    [Parameter, EditorRequired] public string RightHeading { get; set; } = "";
    [Parameter, EditorRequired] public string RightBody { get; set; } = "";
}
