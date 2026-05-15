#pragma warning disable S1144, S4487, S2933

using Microsoft.AspNetCore.Components;
using RSD.Web.Data.Entities;

namespace RSD.Web.Components.Admin.Shared;

public partial class SeoMetaPanel : ComponentBase
{
    [Parameter, EditorRequired] public SeoMetadata Value { get; set; } = new();
    [Parameter] public EventCallback<SeoMetadata> ValueChanged { get; set; }

    private string PanelId { get; } = $"seo-{Guid.NewGuid():N}";

    private Task OnMetaTitleInput(ChangeEventArgs e) => EmitAsync(Value with { MetaTitle = e.Value?.ToString() ?? "" });
    private Task OnMetaDescriptionInput(ChangeEventArgs e) => EmitAsync(Value with { MetaDescription = e.Value?.ToString() ?? "" });
    private Task OnOgImageInput(ChangeEventArgs e) => EmitAsync(Value with { OgImagePath = e.Value?.ToString() ?? "" });

    private Task OnOgImageUploaded(UploadedFile? file) =>
        file is null ? Task.CompletedTask : EmitAsync(Value with { OgImagePath = file.Path });

    private Task OnOgImageAltChanged(string alt) => EmitAsync(Value with { OgImageAlt = alt });

    private async Task EmitAsync(SeoMetadata next)
    {
        Value = next;
        await ValueChanged.InvokeAsync(next);
    }
}
