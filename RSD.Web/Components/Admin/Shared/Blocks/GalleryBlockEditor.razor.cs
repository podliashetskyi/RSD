#pragma warning disable S1144, S4487, S2933

using Microsoft.AspNetCore.Components;
using RSD.Web.Components.Admin.Shared.BlockRows;

namespace RSD.Web.Components.Admin.Shared.Blocks;

public partial class GalleryBlockEditor : ComponentBase
{
    [Parameter, EditorRequired] public GalleryRow Row { get; set; } = new();

    private void OnTagsChanged(List<string> tags) => Row.Tags = tags;
    private void OnImagesChanged(List<GalleryImageRow> images) => Row.Images = images;
}
