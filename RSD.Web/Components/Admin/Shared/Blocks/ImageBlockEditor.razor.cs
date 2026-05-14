#pragma warning disable S1144, S4487, S2933

using Microsoft.AspNetCore.Components;
using RSD.Web.Components.Admin.Shared.BlockRows;
using RSD.Web.Data.Entities;

namespace RSD.Web.Components.Admin.Shared.Blocks;

public partial class ImageBlockEditor : ComponentBase
{
    [Parameter, EditorRequired] public ImageRow Row { get; set; } = new();

    private void OnUploaded(UploadedFile? file) { if (file is not null) Row.ImagePath = file.Path; }
    private void Clear() => Row.ImagePath = "";
}
