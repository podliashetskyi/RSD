#pragma warning disable S1144, S4487, S2933

using Microsoft.AspNetCore.Components;
using RSD.Web.Data.Entities;

namespace RSD.Web.Components.Admin.Shared;

public partial class ResponsiveImage : ComponentBase
{
    [Parameter] public UploadedFile? File { get; set; }
    [Parameter] public ImageRole Role { get; set; } = ImageRole.Card;
    [Parameter] public string Alt { get; set; } = "";
    [Parameter] public string CssClass { get; set; } = "";
    [Parameter] public string? FallbackSrc { get; set; }

    private bool IsResponsive => Role == ImageRole.Responsive;

    private string? SelectedVariant => File is null ? null : ResolveVariantPath(File, Role);

    private static readonly Dictionary<ImageRole, string> PreferredSizes = new()
    {
        [ImageRole.Avatar] = "small",
        [ImageRole.Card] = "medium",
        [ImageRole.Hero] = "large",
        [ImageRole.Og] = "large",
    };

    private static string ResolveVariantPath(UploadedFile file, ImageRole role) =>
        PreferredSizes.TryGetValue(role, out var size) ? VariantPath(file, size) : file.Path;

    private static string VariantPath(UploadedFile file, string size) =>
        Variant(size, file)?.Path ?? file.Path;

    private static ImageVariant? Variant(string size, UploadedFile file) =>
        file.Variants.FirstOrDefault(v => v.Size == size);
}
