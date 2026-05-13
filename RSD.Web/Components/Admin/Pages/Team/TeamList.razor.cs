#pragma warning disable S1144, S4487, S2933

using Microsoft.AspNetCore.Components;
using RSD.Web.Components.Admin.Shared;
using RSD.Web.Data.Entities;
using RSD.Web.Services.Common;
using RSD.Web.Services.Content;

namespace RSD.Web.Components.Admin.Pages.Team;

public partial class TeamList(ITeamMemberService Service, IToastService Toasts) : ComponentBase
{
    private List<TeamMember> Items { get; set; } = [];
    private string Search { get; set; } = "";

    protected override Task OnInitializedAsync() => ReloadAsync();

    private async Task ReloadAsync()
    {
        var list = await Service.ListAsync(new ContentQuery(Search: Search, PageSize: 200), CancellationToken.None);
        Items = list.OrderBy(m => m.IsManagement).ThenBy(m => m.DisplayOrder).ThenBy(m => m.Name).ToList();
    }

    private Task MoveUpAsync(Guid id) => ReorderAsync(id, -1);

    private Task MoveDownAsync(Guid id) => ReorderAsync(id, +1);

    private async Task ReorderAsync(Guid id, int delta)
    {
        var index = Items.FindIndex(m => m.Id == id);
        var target = index + delta;
        if (index < 0 || target < 0 || target >= Items.Count) return;
        (Items[index], Items[target]) = (Items[target], Items[index]);
        var entries = Items.Select((m, i) => new ReorderEntry(m.Id, i + 1)).ToList();
        var result = await Service.BulkReorderAsync(entries, CancellationToken.None);
        ApplyOutcome(result, "Order saved.");
        await ReloadAsync();
    }

    private async Task DeleteAsync(Guid id)
    {
        var result = await Service.SoftDeleteAsync(id, CancellationToken.None);
        ApplyOutcome(result, "Member deleted.");
        await ReloadAsync();
    }

    private void ApplyOutcome<T>(Result<T> result, string successMessage)
    {
        if (result.Ok) Toasts.Show(successMessage, ToastKind.Success);
        else Toasts.Show(result.Error, ToastKind.Error);
    }
}
