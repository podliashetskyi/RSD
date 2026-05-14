#pragma warning disable S1144, S4487, S2933

using Microsoft.AspNetCore.Components;
using RSD.Web.Data.Entities;
using RSD.Web.Services.Audit;

namespace RSD.Web.Components.Admin.Pages.Audit;

public partial class AuditList(IAuditLog Log) : ComponentBase
{
    private const int PageSize = 50;
    private static readonly AuditAction[] Actions = Enum.GetValues<AuditAction>();

    private List<AuditLogEntry> Items { get; set; } = [];
    private string UserEmail { get; set; } = "";
    private string EntityType { get; set; } = "";
    private AuditAction? ActionFilter { get; set; }
    private DateTime? FromDate { get; set; }
    private DateTime? ToDate { get; set; }
    private int Page { get; set; } = 1;
    private bool HasNextPage { get; set; }
    private Guid ExpandedId { get; set; }

    protected override Task OnInitializedAsync() => ReloadAsync();

    private async Task ReloadAsync()
    {
        var query = new AuditQuery(
            UserEmail: UserEmail,
            EntityType: EntityType,
            Action: ActionFilter,
            From: FromDate is { } from ? DateOnly.FromDateTime(from) : null,
            To: ToDate is { } to ? DateOnly.FromDateTime(to) : null,
            Page: Page,
            PageSize: PageSize + 1);
        var rows = await Log.ListAsync(query, CancellationToken.None);
        HasNextPage = rows.Count > PageSize;
        Items = [.. rows.Take(PageSize)];
    }

    private async Task OnActionChangedAsync(AuditAction? next)
    {
        ActionFilter = next;
        Page = 1;
        await ReloadAsync();
    }

    private async Task PrevAsync()
    {
        if (Page <= 1) return;
        Page--;
        await ReloadAsync();
    }

    private async Task NextAsync()
    {
        if (!HasNextPage) return;
        Page++;
        await ReloadAsync();
    }

    private void ToggleExpanded(Guid id) => ExpandedId = ExpandedId == id ? Guid.Empty : id;
}
