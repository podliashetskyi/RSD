#pragma warning disable S1144, S4487, S2933

using Microsoft.AspNetCore.Components;

namespace RSD.Web.Components.Admin.Shared;

public partial class ConstrainedTagPicker : ComponentBase
{
    [Parameter, EditorRequired] public string Label { get; set; } = "";
    [Parameter] public string Hint { get; set; } = "";
    [Parameter] public string EmptyOptionsText { get; set; } = "No options available yet.";
    [Parameter] public string ManageHref { get; set; } = "";
    [Parameter] public List<string> Value { get; set; } = [];
    [Parameter] public EventCallback<List<string>> ValueChanged { get; set; }
    [Parameter] public IReadOnlyList<string> Options { get; set; } = [];

    private bool IsSelected(string option) =>
        Value.Contains(option, StringComparer.OrdinalIgnoreCase);

    private async Task ToggleAsync(string option)
    {
        var next = new List<string>(Value);
        var existing = next.FirstOrDefault(v => string.Equals(v, option, StringComparison.OrdinalIgnoreCase));
        if (existing is not null) next.Remove(existing);
        else next.Add(option);
        Value = next;
        await ValueChanged.InvokeAsync(next);
    }
}
