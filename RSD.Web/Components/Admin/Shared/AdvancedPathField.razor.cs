#pragma warning disable S1144, S4487, S2933

using Microsoft.AspNetCore.Components;

namespace RSD.Web.Components.Admin.Shared;

public partial class AdvancedPathField : ComponentBase
{
    [Parameter] public string Summary { get; set; } = "Advanced: edit stored path";
    [Parameter] public string Label { get; set; } = "Stored path";
    [Parameter] public string Hint { get; set; } = "Use only when reusing an existing uploaded asset.";
    [Parameter] public RenderFragment? ChildContent { get; set; }
}
