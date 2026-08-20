#pragma warning disable S1144, S4487, S2933
using Microsoft.AspNetCore.Components;

namespace RSD.Web.Components.Sections.Shared;

/// <summary>One related item: a small taxonomy label, the linked title, and its href.</summary>
public sealed record RelatedLink(string Label, string Title, string Href);

public partial class RelatedContentSection
{
    [Parameter, EditorRequired] public string Heading { get; set; } = "";
    [Parameter, EditorRequired] public IReadOnlyList<RelatedLink> Items { get; set; } = [];
}
