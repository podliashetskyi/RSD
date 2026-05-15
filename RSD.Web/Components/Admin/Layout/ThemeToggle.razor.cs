#pragma warning disable S1144, S4487, S2933

using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace RSD.Web.Components.Admin.Layout;

public partial class ThemeToggle(IJSRuntime Js) : ComponentBase, IAsyncDisposable
{
    private string Theme { get; set; } = "light";
    private IJSObjectReference? Module { get; set; }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (!firstRender) return;
        Module = await Js.InvokeAsync<IJSObjectReference>("import", "/js/admin/theme-toggle.js");
        Theme = await Module.InvokeAsync<string>("getResolvedTheme");
        StateHasChanged();
    }

    private async Task ToggleAsync()
    {
        if (Module is null) return;
        Theme = Theme == "dark" ? "light" : "dark";
        await Module.InvokeVoidAsync("setTheme", Theme);
    }

    public async ValueTask DisposeAsync()
    {
        if (Module is null) return;
        try { await Module.DisposeAsync(); }
        catch { /* component already gone */ }
    }
}
