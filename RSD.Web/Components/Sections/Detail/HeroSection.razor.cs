#pragma warning disable S1144, S4487, S2933
using Microsoft.AspNetCore.Components;

namespace RSD.Web.Components.Sections.Detail;

public partial class HeroSection
{
    [Parameter, EditorRequired] public string ImageSrc { get; set; } = "";
    [Parameter] public string Alt { get; set; } = "";
}
