#pragma warning disable S1144, S4487, S2933
using Microsoft.AspNetCore.Components;

namespace RSD.Web.Components.Sections.Article;

public partial class GallerySection
{
    [Parameter] public string Id { get; set; } = "";
    [Parameter, EditorRequired] public string Heading { get; set; } = "";
    [Parameter] public string Description { get; set; } = "";
    [Parameter] public string PrimaryButtonText { get; set; } = "Read more";
    [Parameter] public string PrimaryButtonHref { get; set; } = "#";
    [Parameter] public string SecondaryButtonText { get; set; } = "View our team";
    [Parameter] public string SecondaryButtonHref { get; set; } = "#";
    [Parameter, EditorRequired] public IReadOnlyList<GalleryImage> Images { get; set; } = [];
    [Parameter] public IReadOnlyList<string> Tags { get; set; } = [];
}

public record GalleryImage(string Src, string Alt);
