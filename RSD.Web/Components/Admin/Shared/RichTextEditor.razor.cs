#pragma warning disable S1144, S4487, S2933

using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace RSD.Web.Components.Admin.Shared;

public partial class RichTextEditor(IJSRuntime Js) : ComponentBase, IAsyncDisposable
{
    [Parameter] public string Label { get; set; } = "";
    [Parameter] public string Hint { get; set; } = "";
    [Parameter] public string Value { get; set; } = "";
    [Parameter] public EventCallback<string> ValueChanged { get; set; }

    private ElementReference ContainerRef { get; set; }
    private IJSObjectReference? Module { get; set; }
    private IJSObjectReference? Editor { get; set; }
    private DotNetObjectReference<RichTextEditor>? Self { get; set; }
    private string LastSeenValue { get; set; } = "";

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            Module = await Js.InvokeAsync<IJSObjectReference>("import", "/js/admin/quill-interop.js");
            Self = DotNetObjectReference.Create(this);
            Editor = await Module.InvokeAsync<IJSObjectReference>("attach", ContainerRef, Self, Value);
            LastSeenValue = Value;
            return;
        }

        if (Editor is not null && Value != LastSeenValue)
        {
            LastSeenValue = Value;
            await Editor.InvokeVoidAsync("setValue", Value);
        }
    }

    [JSInvokable]
    public async Task OnHtmlChangedAsync(string html)
    {
        LastSeenValue = html;
        Value = html;
        await ValueChanged.InvokeAsync(html);
    }

    public async ValueTask DisposeAsync()
    {
        if (Editor is not null)
        {
            try { await Editor.InvokeVoidAsync("destroy"); }
            catch { /* component disposed */ }
            await Editor.DisposeAsync();
        }
        if (Module is not null) await Module.DisposeAsync();
        Self?.Dispose();
    }
}
