#pragma warning disable S1144, S4487, S2933

using System.Globalization;
using System.Security.Claims;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using RSD.Web.Components.Admin.Shared;
using RSD.Web.Data.Entities;
using RSD.Web.Services.Estimates;

namespace RSD.Web.Components.Admin.Pages.Estimates;

public partial class EstimateList(
    IProjectEstimateService Service,
    AuthenticationStateProvider AuthState,
    IToastService Toasts) : ComponentBase
{
    private const int EstimatesPageSize = 25;

    private List<ProjectEstimate> Items { get; set; } = [];
    private int Total { get; set; }
    private int Open { get; set; }
    private int Page { get; set; } = 1;
    private int TotalPages => Math.Max(1, (int)Math.Ceiling(Total / (double)EstimatesPageSize));
    private string Search { get; set; } = "";
    private ProjectEstimateFilter Filter { get; set; } = ProjectEstimateFilter.Open;
    private ProjectEstimate? Selected { get; set; }
    private bool DeleteDialogOpen { get; set; }
    private string ReplyMailto => Selected is null ? "#" : BuildMailto(Selected);
    private string DeleteDialogBody => Selected is null
        ? ""
        : $"This will permanently delete the estimate from {Selected.ContactName} <{Selected.ContactEmail}>. This action cannot be undone.";

    private static readonly IReadOnlyList<FilterOption> FilterOptions =
    [
        new("Open", ProjectEstimateFilter.Open),
        new("Handled", ProjectEstimateFilter.Handled),
        new("All", ProjectEstimateFilter.All),
    ];

    protected override Task OnInitializedAsync() => ReloadAsync();

    private async Task ReloadAsync()
    {
        var query = new ProjectEstimateQuery(Filter, Search, Page, EstimatesPageSize);
        var result = await Service.ListAsync(query, CancellationToken.None);
        Items = [.. result.Items];
        Total = result.TotalCount;
        if (Page > TotalPages) Page = TotalPages;
        var openPage = await Service.ListAsync(new ProjectEstimateQuery(ProjectEstimateFilter.Open, "", 1, 1), CancellationToken.None);
        Open = openPage.TotalCount;
    }

    private async Task SetFilterAsync(ProjectEstimateFilter filter)
    {
        if (Filter == filter) return;
        Filter = filter;
        Page = 1;
        await ReloadAsync();
    }

    private string FilterButtonClass(ProjectEstimateFilter filter) =>
        Filter == filter
            ? "rounded-md border border-gray-900 dark:border-white bg-gray-900 dark:bg-white px-3 py-1.5 text-sm font-medium text-white dark:text-gray-900"
            : "rounded-md border border-gray-300 dark:border-gray-700 px-3 py-1.5 text-sm text-gray-700 dark:text-gray-300 hover:bg-gray-50 dark:hover:bg-gray-800";

    private async Task PreviousPageAsync()
    {
        if (Page <= 1) return;
        Page--;
        await ReloadAsync();
    }

    private async Task NextPageAsync()
    {
        if (Page >= TotalPages) return;
        Page++;
        await ReloadAsync();
    }

    private async Task OpenDetailAsync(Guid id)
    {
        Selected = await Service.GetByIdAsync(id, CancellationToken.None);
    }

    private void CloseDetail() => Selected = null;

    private async Task MarkHandledAsync()
    {
        if (Selected is null) return;
        var userId = await ResolveUserIdAsync();
        var result = await Service.MarkHandledAsync(Selected.Id, userId, CancellationToken.None);
        ApplyOutcome(result, "Marked as handled.");
        Selected = await Service.GetByIdAsync(Selected.Id, CancellationToken.None);
        await ReloadAsync();
    }

    private async Task MarkOpenAsync()
    {
        if (Selected is null) return;
        var result = await Service.MarkOpenAsync(Selected.Id, CancellationToken.None);
        ApplyOutcome(result, "Reopened.");
        Selected = await Service.GetByIdAsync(Selected.Id, CancellationToken.None);
        await ReloadAsync();
    }

    private void RequestDelete()
    {
        if (Selected is not null) DeleteDialogOpen = true;
    }

    private async Task ConfirmDeleteAsync()
    {
        if (Selected is null) return;
        var result = await Service.DeleteAsync(Selected.Id, CancellationToken.None);
        ApplyOutcome(result, "Estimate deleted.");
        if (!result.Ok) return;
        Selected = null;
        await ReloadAsync();
    }

    private void CancelDelete() => DeleteDialogOpen = false;

    private void OnDeleteDialogOpenChanged(bool open) => DeleteDialogOpen = open;

    private async Task<string> ResolveUserIdAsync()
    {
        var state = await AuthState.GetAuthenticationStateAsync();
        return state.User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "";
    }

    private void ApplyOutcome<T>(RSD.Web.Services.Common.Result<T> result, string successMessage)
    {
        if (result.Ok) Toasts.Show(successMessage, ToastKind.Success);
        else Toasts.Show(result.Error, ToastKind.Error);
    }

    private static string FormatMoney(decimal value) => value.ToString("N0", CultureInfo.InvariantCulture);

    private static string BuildMailto(ProjectEstimate estimate)
    {
        var subject = Uri.EscapeDataString($"Your project estimate: {estimate.Company}");
        var body = Uri.EscapeDataString(
            $"\n\n---\nSubmitted {estimate.SubmittedAt:u}\n"
            + $"Selections: {EstimatorCatalog.SummaryChip(estimate.Platform, estimate.Domain, estimate.Complexity, estimate.Timeline)}\n"
            + $"Preliminary range: $ {estimate.EstimateMin:N0} – $ {estimate.EstimateMax:N0}\n\n"
            + $"{estimate.ProjectDescription}");
        return $"mailto:{Uri.EscapeDataString(estimate.ContactEmail)}?subject={subject}&body={body}";
    }

    private sealed record FilterOption(string Label, ProjectEstimateFilter Value);
}
