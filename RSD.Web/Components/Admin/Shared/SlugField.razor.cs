#pragma warning disable S1144, S4487, S2933

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using RSD.Web.Data.Entities;
using RSD.Web.Services.Slugs;

namespace RSD.Web.Components.Admin.Shared;

public partial class SlugField<TEntity>(ISlugger Slugger) : ComponentBase where TEntity : ContentEntity
{
    [Parameter] public string Label { get; set; } = "URL path";
    [Parameter] public string Value { get; set; } = "";
    [Parameter] public string TitleSource { get; set; } = "";
    [Parameter] public Guid? CurrentEntityId { get; set; }
    [Parameter] public bool Locked { get; set; } = true;
    [Parameter] public EventCallback<string> ValueChanged { get; set; }
    [Parameter] public EventCallback<bool> LockedChanged { get; set; }
    [Parameter] public EventCallback<bool> IsValidChanged { get; set; }

    private string CurrentValue { get; set; } = "";
    private string Message { get; set; } = "";
    private bool IsAvailable { get; set; } = true;
    private bool Initialized { get; set; }
    private string FieldId { get; } = $"slug-{Guid.NewGuid():N}";

    private string MessageClass => IsAvailable ? "text-gray-500 dark:text-gray-400" : "text-red-600 dark:text-red-400";

    protected override async Task OnParametersSetAsync()
    {
        // First render with real data. For an existing entity loaded async, this is the
        // post-LoadAsync render. Preserve the loaded slug verbatim (it may be a custom
        // slug that differs from Slugify(Title)) and auto-unlock so subsequent title
        // edits don't clobber the persisted URL.
        if (!Initialized && !string.IsNullOrEmpty(Value))
        {
            Initialized = true;
            CurrentValue = Value;
            if (Locked)
            {
                Locked = false;
                await LockedChanged.InvokeAsync(false);
            }
            await CheckAvailabilityAsync(CurrentValue);
            return;
        }
        if (!Initialized && !string.IsNullOrEmpty(TitleSource)) Initialized = true;
        var derived = Locked ? Slugger.Slugify(TitleSource) : Value;
        if (derived == CurrentValue) return;
        CurrentValue = derived;
        await PropagateValueAsync();
        await CheckAvailabilityAsync(CurrentValue);
    }

    private async Task HandleInputAsync(ChangeEventArgs e)
    {
        var raw = e.Value?.ToString() ?? "";
        CurrentValue = Slugger.Slugify(raw);
        await PropagateValueAsync();
    }

    private Task HandleBlurAsync(FocusEventArgs _) => CheckAvailabilityAsync(CurrentValue);

    private async Task ToggleLock()
    {
        Locked = !Locked;
        await LockedChanged.InvokeAsync(Locked);
        if (Locked) await RederiveFromTitleAsync();
    }

    private async Task RederiveFromTitleAsync()
    {
        CurrentValue = Slugger.Slugify(TitleSource);
        await PropagateValueAsync();
        await CheckAvailabilityAsync(CurrentValue);
    }

    private Task PropagateValueAsync() => ValueChanged.InvokeAsync(CurrentValue);

    private async Task CheckAvailabilityAsync(string slug)
    {
        if (string.IsNullOrWhiteSpace(slug))
        {
            await ApplyAvailabilityAsync(false, "URL path is required.");
            return;
        }
        var available = await Slugger.IsAvailableAsync<TEntity>(slug, CurrentEntityId, CancellationToken.None);
        await ApplyAvailabilityAsync(available, available ? "" : "This URL path is already in use.");
    }

    private async Task ApplyAvailabilityAsync(bool available, string message)
    {
        IsAvailable = available;
        Message = message;
        await IsValidChanged.InvokeAsync(available);
        StateHasChanged();
    }
}
