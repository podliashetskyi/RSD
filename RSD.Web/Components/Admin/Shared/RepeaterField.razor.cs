#pragma warning disable S1144, S4487, S2933

using Microsoft.AspNetCore.Components;

namespace RSD.Web.Components.Admin.Shared;

public partial class RepeaterField<TRow> : ComponentBase where TRow : class, new()
{
    [Parameter, EditorRequired] public string Label { get; set; } = "";
    [Parameter] public string Hint { get; set; } = "";
    [Parameter] public string AddLabel { get; set; } = "Add row";
    [Parameter] public string EmptyText { get; set; } = "No rows yet.";

    [Parameter, EditorRequired] public List<TRow> Items { get; set; } = [];
    [Parameter] public EventCallback<List<TRow>> ItemsChanged { get; set; }

    [Parameter, EditorRequired] public RenderFragment<TRow> RowTemplate { get; set; } = default!;
    [Parameter] public Func<TRow>? NewItem { get; set; }

    private Task AddAsync() => EmitAsync([.. Items, NewItem?.Invoke() ?? new TRow()]);

    private Task RemoveAsync(int index)
    {
        if (index < 0 || index >= Items.Count) return Task.CompletedTask;
        var next = new List<TRow>(Items);
        next.RemoveAt(index);
        return EmitAsync(next);
    }

    private Task MoveUpAsync(int index) => index <= 0 ? Task.CompletedTask : SwapAsync(index, index - 1);

    private Task MoveDownAsync(int index) => index < 0 || index >= Items.Count - 1 ? Task.CompletedTask : SwapAsync(index, index + 1);

    private Task SwapAsync(int a, int b)
    {
        var next = new List<TRow>(Items);
        (next[a], next[b]) = (next[b], next[a]);
        return EmitAsync(next);
    }

    private async Task EmitAsync(List<TRow> next)
    {
        Items = next;
        await ItemsChanged.InvokeAsync(next);
    }
}
