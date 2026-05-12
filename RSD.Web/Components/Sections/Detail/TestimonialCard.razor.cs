#pragma warning disable S1144, S4487, S2933
using Microsoft.AspNetCore.Components;

namespace RSD.Web.Components.Sections.Detail;

public partial class TestimonialCard
{
    [Parameter, EditorRequired] public string Quote { get; set; } = "";
    [Parameter, EditorRequired] public string AuthorName { get; set; } = "";
    [Parameter, EditorRequired] public string AuthorRole { get; set; } = "";
}
