#pragma warning disable S1144, S4487, S2933

using Microsoft.AspNetCore.Components;
using RSD.Web.Components.Admin.Shared.BlockRows;

namespace RSD.Web.Components.Admin.Shared.Blocks;

public partial class RichTextBlockEditor : ComponentBase
{
    [Parameter, EditorRequired] public RichTextRow Row { get; set; } = new();

    private void OnHtmlChanged(string html) => Row.Html = html;
}
