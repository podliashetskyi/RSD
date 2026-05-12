#pragma warning disable S1144, S4487, S2933
using Microsoft.AspNetCore.Components;

namespace RSD.Web.Components.Sections.Article;

public partial class ArticleBodySection
{
    [Parameter] public IReadOnlyList<TocEntry> TocItems { get; set; } = [];
    [Parameter] public RenderFragment? ChildContent { get; set; }
}

public record TocEntry(string AnchorId, string Label);
