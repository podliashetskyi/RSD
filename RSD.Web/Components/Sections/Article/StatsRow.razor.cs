#pragma warning disable S1144, S4487, S2933
using Microsoft.AspNetCore.Components;

namespace RSD.Web.Components.Sections.Article;

public partial class StatsRow
{
    [Parameter] public string Id { get; set; } = "";
    [Parameter] public string Heading { get; set; } = "";
    [Parameter, EditorRequired] public IReadOnlyList<StatRowItem> Items { get; set; } = [];
}

public record StatRowItem(string Number, string Label);
