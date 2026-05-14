#pragma warning disable S1144, S4487, S2933

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace RSD.Web.Components.Admin.Shared;

public partial class TagInput : ComponentBase
{
    [Parameter] public string Label { get; set; } = "Tags";
    [Parameter] public string Hint { get; set; } = "Press Enter or comma to add a tag.";
    [Parameter] public string Placeholder { get; set; } = "Add tag…";
    [Parameter] public List<string> Value { get; set; } = [];
    [Parameter] public EventCallback<List<string>> ValueChanged { get; set; }

    private string Draft { get; set; } = "";
    private string FieldId { get; } = $"tags-{Guid.NewGuid():N}";

    private async Task OnKeyDownAsync(KeyboardEventArgs e)
    {
        if (e.Key == "Enter" || e.Key == ",")
        {
            await TryAddAsync();
            return;
        }
        if (e.Key == "Backspace" && string.IsNullOrEmpty(Draft) && Value.Count > 0)
        {
            await RemoveAtAsync(Value.Count - 1);
        }
    }

    private async Task TryAddAsync()
    {
        var candidate = Draft.Trim().TrimEnd(',');
        if (string.IsNullOrWhiteSpace(candidate)) return;
        var next = new List<string>(Value);
        if (!next.Contains(candidate, StringComparer.OrdinalIgnoreCase)) next.Add(candidate);
        Draft = "";
        await EmitAsync(next);
    }

    private async Task RemoveAtAsync(int index)
    {
        if (index < 0 || index >= Value.Count) return;
        var next = new List<string>(Value);
        next.RemoveAt(index);
        await EmitAsync(next);
    }

    private async Task EmitAsync(List<string> next)
    {
        Value = next;
        await ValueChanged.InvokeAsync(next);
    }
}
