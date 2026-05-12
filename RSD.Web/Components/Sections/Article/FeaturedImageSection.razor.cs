#pragma warning disable S1144, S4487, S2933
using Microsoft.AspNetCore.Components;

namespace RSD.Web.Components.Sections.Article;

public partial class FeaturedImageSection
{
    [Parameter, EditorRequired] public string ImageSrc { get; set; } = "";
    [Parameter] public string Caption { get; set; } = "";
}
