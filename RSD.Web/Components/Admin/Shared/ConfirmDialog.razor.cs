#pragma warning disable S1144, S4487, S2933

using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace RSD.Web.Components.Admin.Shared;

public partial class ConfirmDialog(IJSRuntime Js) : ComponentBase, IAsyncDisposable
{
    [Parameter] public bool IsOpen { get; set; }
    [Parameter] public string Title { get; set; } = "Confirm";
    [Parameter] public string Body { get; set; } = "";
    [Parameter] public string ConfirmLabel { get; set; } = "Confirm";
    [Parameter] public string CancelLabel { get; set; } = "Cancel";
    [Parameter] public bool IsDestructive { get; set; }
    [Parameter] public string RequiredText { get; set; } = "";
    [Parameter] public EventCallback OnConfirm { get; set; }
    [Parameter] public EventCallback OnCancel { get; set; }
    [Parameter] public EventCallback<bool> IsOpenChanged { get; set; }

    private string TitleId { get; } = $"dlg-{Guid.NewGuid():N}";
    private string TypedConfirmation { get; set; } = "";
    private ElementReference DialogRef { get; set; }
    private IJSObjectReference? JsModule { get; set; }
    private DotNetObjectReference<ConfirmDialog>? Self { get; set; }
    private bool FocusAttached { get; set; }

    private string ConfirmClasses => IsDestructive
        ? "bg-red-600 hover:bg-red-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-red-500"
        : "bg-gray-900 hover:bg-black dark:bg-white dark:text-gray-900 dark:hover:bg-gray-100 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-gray-500";

    private bool CanConfirm => string.IsNullOrEmpty(RequiredText)
        || string.Equals(TypedConfirmation, RequiredText, StringComparison.Ordinal);

    protected override void OnParametersSet()
    {
        if (!IsOpen) TypedConfirmation = "";
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (IsOpen && !FocusAttached) await AttachFocusAsync();
        if (!IsOpen && FocusAttached) await DetachFocusAsync();
    }

    [JSInvokable]
    public Task HandleEscapeAsync() => CancelAsync();

    private async Task ConfirmAsync()
    {
        if (!CanConfirm) return;
        await OnConfirm.InvokeAsync();
        await CloseAsync();
    }

    private async Task CancelAsync()
    {
        await OnCancel.InvokeAsync();
        await CloseAsync();
    }

    private async Task CloseAsync()
    {
        await DetachFocusAsync();
        IsOpen = false;
        await IsOpenChanged.InvokeAsync(false);
    }

    private async Task AttachFocusAsync()
    {
        JsModule ??= await Js.InvokeAsync<IJSObjectReference>("import", "/js/admin/modal-focus.js");
        Self ??= DotNetObjectReference.Create(this);
        await JsModule.InvokeVoidAsync("attach", DialogRef, Self, nameof(HandleEscapeAsync));
        FocusAttached = true;
    }

    private async Task DetachFocusAsync()
    {
        if (!FocusAttached || JsModule is null) return;
        await JsModule.InvokeVoidAsync("detach", DialogRef);
        FocusAttached = false;
    }

    public async ValueTask DisposeAsync()
    {
        await DetachFocusAsync();
        if (JsModule is not null) await JsModule.DisposeAsync();
        Self?.Dispose();
    }
}
