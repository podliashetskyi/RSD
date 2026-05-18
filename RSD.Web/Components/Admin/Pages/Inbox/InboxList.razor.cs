#pragma warning disable S1144, S4487, S2933

using System.Security.Claims;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;
using RSD.Web.Components.Admin.Shared;
using RSD.Web.Data.Entities;
using RSD.Web.Services.Content;

namespace RSD.Web.Components.Admin.Pages.Inbox;

public partial class InboxList(
    IContactSubmissionService Service,
    AuthenticationStateProvider AuthState,
    IToastService Toasts,
    IJSRuntime Js) : ComponentBase, IAsyncDisposable
{
    private const int InboxPageSize = 25;

    private List<ContactSubmission> Items { get; set; } = [];
    private int Total { get; set; }
    private int Open { get; set; }
    private int Page { get; set; } = 1;
    private int TotalPages => Math.Max(1, (int)Math.Ceiling(Total / (double)InboxPageSize));
    private string Search { get; set; } = "";
    private ContactSubmissionFilter Filter { get; set; } = ContactSubmissionFilter.Open;
    private ContactSubmission? Selected { get; set; }
    private bool DeleteDialogOpen { get; set; }
    private ElementReference DetailDialogRef { get; set; }
    private IJSObjectReference? JsModule { get; set; }
    private DotNetObjectReference<InboxList>? Self { get; set; }
    private bool DetailFocusAttached { get; set; }
    private string ReplyMailto => Selected is null ? "#" : BuildMailto(Selected);
    private string DeleteDialogBody => Selected is null
        ? ""
        : $"This will permanently delete the submission from {Selected.Name} <{Selected.Email}>. This action cannot be undone.";

    private static readonly IReadOnlyList<FilterOption> FilterOptions =
    [
        new("Open", ContactSubmissionFilter.Open),
        new("Handled", ContactSubmissionFilter.Handled),
        new("All", ContactSubmissionFilter.All),
    ];

    protected override Task OnInitializedAsync() => ReloadAsync();

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (Selected is not null && !DetailFocusAttached) await AttachDetailFocusAsync();
        if (Selected is null && DetailFocusAttached) await DetachDetailFocusAsync();
    }

    [JSInvokable]
    public Task HandleDetailEscapeAsync() => CloseDetail();

    private async Task ReloadAsync()
    {
        var query = new ContactSubmissionQuery(Filter, Search, Page, InboxPageSize);
        var result = await Service.ListAsync(query, CancellationToken.None);
        Items = [.. result.Items];
        Total = result.TotalCount;
        if (Page > TotalPages) Page = TotalPages;
        var openPage = await Service.ListAsync(new ContactSubmissionQuery(ContactSubmissionFilter.Open, "", 1, 1), CancellationToken.None);
        Open = openPage.TotalCount;
    }

    private async Task SetFilterAsync(ContactSubmissionFilter filter)
    {
        if (Filter == filter) return;
        Filter = filter;
        Page = 1;
        await ReloadAsync();
    }

    private string FilterButtonClass(ContactSubmissionFilter filter) =>
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

    private async Task CloseDetail()
    {
        Selected = null;
        await DetachDetailFocusAsync();
    }

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
        ApplyOutcome(result, "Submission deleted.");
        if (!result.Ok) return;
        Selected = null;
        await DetachDetailFocusAsync();
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

    private static string BuildMailto(ContactSubmission submission)
    {
        var subject = Uri.EscapeDataString($"Re: {submission.Subject}");
        var body = Uri.EscapeDataString($"\n\n---\nOn {submission.SubmittedAt:u}, {submission.Name} wrote:\n{submission.Message}");
        return $"mailto:{Uri.EscapeDataString(submission.Email)}?subject={subject}&body={body}";
    }

    private async Task AttachDetailFocusAsync()
    {
        JsModule ??= await Js.InvokeAsync<IJSObjectReference>("import", "/js/admin/modal-focus.js");
        Self ??= DotNetObjectReference.Create(this);
        await JsModule.InvokeVoidAsync("attach", DetailDialogRef, Self, nameof(HandleDetailEscapeAsync));
        DetailFocusAttached = true;
    }

    private async Task DetachDetailFocusAsync()
    {
        if (!DetailFocusAttached || JsModule is null) return;
        await JsModule.InvokeVoidAsync("detach", DetailDialogRef);
        DetailFocusAttached = false;
    }

    public async ValueTask DisposeAsync()
    {
        await DetachDetailFocusAsync();
        if (JsModule is not null) await JsModule.DisposeAsync();
        Self?.Dispose();
    }

    private sealed record FilterOption(string Label, ContactSubmissionFilter Value);
}
