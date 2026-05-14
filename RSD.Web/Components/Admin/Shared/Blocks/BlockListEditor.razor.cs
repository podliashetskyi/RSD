#pragma warning disable S1144, S4487, S2933

using Microsoft.AspNetCore.Components;
using RSD.Web.Components.Admin.Shared.BlockRows;

namespace RSD.Web.Components.Admin.Shared.Blocks;

public partial class BlockListEditor : ComponentBase
{
    [Parameter] public string Label { get; set; } = "Body blocks";
    [Parameter, EditorRequired] public List<IBlockRow> Rows { get; set; } = [];
    [Parameter] public EventCallback<List<IBlockRow>> RowsChanged { get; set; }

    private bool MenuOpen { get; set; }
    private string ExpandedId { get; set; } = "";

    private static readonly BlockKind[] PaletteKinds =
    [
        BlockKind.RichText, BlockKind.Subsection, BlockKind.BulletList,
        BlockKind.Quote, BlockKind.Image, BlockKind.Gallery, BlockKind.StatsRow,
    ];

    private static string LabelFor(BlockKind k) => k switch
    {
        BlockKind.Subsection => "Subsection",
        BlockKind.StatsRow => "Stats row",
        BlockKind.Gallery => "Gallery",
        BlockKind.BulletList => "Bullet list",
        BlockKind.Quote => "Quote",
        BlockKind.Image => "Image",
        BlockKind.RichText => "Rich text",
        _ => k.ToString(),
    };

    private void ToggleMenu() => MenuOpen = !MenuOpen;

    private void ToggleExpanded(string id) => ExpandedId = ExpandedId == id ? "" : id;

    private Task AddAsync(BlockKind kind)
    {
        MenuOpen = false;
        var next = new List<IBlockRow>(Rows) { BlockRowFactory.Create(kind) };
        ExpandedId = next[^1].Id;
        return EmitAsync(next);
    }

    private Task RemoveAsync(int idx)
    {
        if (idx < 0 || idx >= Rows.Count) return Task.CompletedTask;
        var next = new List<IBlockRow>(Rows);
        if (ExpandedId == next[idx].Id) ExpandedId = "";
        next.RemoveAt(idx);
        return EmitAsync(next);
    }

    private Task MoveUpAsync(int idx) => idx <= 0 ? Task.CompletedTask : SwapAsync(idx, idx - 1);
    private Task MoveDownAsync(int idx) => idx < 0 || idx >= Rows.Count - 1 ? Task.CompletedTask : SwapAsync(idx, idx + 1);

    private Task SwapAsync(int a, int b)
    {
        var next = new List<IBlockRow>(Rows);
        (next[a], next[b]) = (next[b], next[a]);
        return EmitAsync(next);
    }

    private async Task EmitAsync(List<IBlockRow> next)
    {
        Rows = next;
        await RowsChanged.InvokeAsync(next);
    }
}
