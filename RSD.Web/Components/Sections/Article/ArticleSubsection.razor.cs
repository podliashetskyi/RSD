#pragma warning disable S1144, S4487, S2933
using Microsoft.AspNetCore.Components;
using RSD.Web.Data.Entities;

namespace RSD.Web.Components.Sections.Article;

public partial class ArticleSubsection
{
    [Parameter] public string Id { get; set; } = "";
    [Parameter] public string Heading { get; set; } = "";
    [Parameter] public string Subheading { get; set; } = "";
    [Parameter] public string SubheadingBody { get; set; } = "";
    [Parameter] public IReadOnlyList<SubsectionItem> Items { get; set; } = [];
    [Parameter] public ArticleListStyle Style { get; set; } = ArticleListStyle.CheckIcon;
}

public enum ArticleListStyle { CheckIcon, Disc, Numbered }
