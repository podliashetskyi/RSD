#pragma warning disable S1144, S4487, S2933

using Microsoft.AspNetCore.Components;

namespace RSD.Web.Components.Admin.Shared;

public partial class FieldField : ComponentBase
{
    [Parameter, EditorRequired] public string Label { get; set; } = "";
    [Parameter] public string Hint { get; set; } = "";
    [Parameter, EditorRequired] public int MaxLength { get; set; }
    [Parameter, EditorRequired] public string Value { get; set; } = "";
    [Parameter] public RenderFragment? ChildContent { get; set; }

    private int Length => Value?.Length ?? 0;

    private string CounterClass =>
        Length >= MaxLength ? "text-red-600 dark:text-red-400 font-medium"
        : Length >= (int)(MaxLength * 0.9) ? "text-amber-600 dark:text-amber-400"
        : "text-gray-500 dark:text-gray-400";
}
