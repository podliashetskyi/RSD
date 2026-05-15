#pragma warning disable S1144, S4487, S2933

using Microsoft.AspNetCore.Components;
using RSD.Web.Components.Admin.Shared;
using RSD.Web.Data.Entities;
using RSD.Web.Services.Common;
using RSD.Web.Services.Content;

namespace RSD.Web.Components.Admin.Pages.Filters;

public partial class FilterList(IFilterService Service, IToastService Toasts) : ComponentBase
{
    private List<Filter> Items { get; set; } = [];
    private FilterType ActiveType { get; set; } = FilterType.CaseIndustry;

    private List<Filter> ShownItems => [.. Items.Where(f => f.Type == ActiveType).OrderBy(f => f.DisplayOrder).ThenBy(f => f.Label)];
    private Dictionary<FilterType, int> CountByType => Items.GroupBy(f => f.Type).ToDictionary(g => g.Key, g => g.Count());
    private string ActiveTypeLabel => TabDefs.First(t => t.Type == ActiveType).Label;

    private static readonly IReadOnlyList<(FilterType Type, string Label)> TabDefs =
    [
        (FilterType.CaseIndustry, "Case industries"),
        (FilterType.CaseTechTag,  "Case tech tags"),
        (FilterType.BlogCategory, "Blog categories"),
        (FilterType.BlogTag,      "Blog tags"),
    ];

    protected override Task OnInitializedAsync() => ReloadAsync();

    private async Task ReloadAsync()
    {
        var list = await Service.ListAsync(new ContentQuery(PageSize: 500), CancellationToken.None);
        Items = [.. list];
    }

    private void SetActiveAsync(FilterType type) => ActiveType = type;

    private Task MoveUpAsync(Guid id) => ReorderAsync(id, -1);
    private Task MoveDownAsync(Guid id) => ReorderAsync(id, +1);

    private async Task ReorderAsync(Guid id, int delta)
    {
        var slice = ShownItems;
        var index = slice.FindIndex(f => f.Id == id);
        var target = index + delta;
        if (index < 0 || target < 0 || target >= slice.Count) return;
        (slice[index], slice[target]) = (slice[target], slice[index]);
        var entries = slice.Select((f, i) => new ReorderEntry(f.Id, i + 1)).ToList();
        ApplyOutcome(await Service.BulkReorderAsync(entries, CancellationToken.None), "Order saved.");
        await ReloadAsync();
    }

    private async Task DeleteAsync(Guid id)
    {
        ApplyOutcome(await Service.SoftDeleteAsync(id, CancellationToken.None), "Filter deleted.");
        await ReloadAsync();
    }

    private void ApplyOutcome<T>(Result<T> result, string successMessage)
    {
        if (result.Ok) Toasts.Show(successMessage, ToastKind.Success);
        else Toasts.Show(result.Error, ToastKind.Error);
    }
}
