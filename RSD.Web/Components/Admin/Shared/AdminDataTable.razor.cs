#pragma warning disable S1144, S4487, S2933

using Microsoft.AspNetCore.Components;

namespace RSD.Web.Components.Admin.Shared;

public partial class AdminDataTable<TItem> : ComponentBase
{
    [Parameter, EditorRequired] public IReadOnlyList<TItem> Items { get; set; } = [];
    [Parameter, EditorRequired] public RenderFragment<TItem> RowTemplate { get; set; } = default!;
    [Parameter] public RenderFragment? ColumnHeaderTemplate { get; set; }
    [Parameter] public RenderFragment? HeaderTemplate { get; set; }
    [Parameter] public RenderFragment? ToolbarTemplate { get; set; }
    [Parameter] public string EmptyText { get; set; } = "No items yet.";
    [Parameter] public string MinWidthClass { get; set; } = "min-w-[56rem]";
}
