#pragma warning disable S1144, S4487, S2933
using Microsoft.AspNetCore.Components;

namespace RSD.Web.Components.Sections.Article;

public partial class ArticleHeaderSection
{
    [Parameter] public string BackHref { get; set; } = "/";
    [Parameter, EditorRequired] public string CategoryText { get; set; } = "";
    [Parameter, EditorRequired] public string DateText { get; set; } = "";
    [Parameter, EditorRequired] public string ReadTime { get; set; } = "";
    [Parameter, EditorRequired] public string Title { get; set; } = "";
    [Parameter, EditorRequired] public string Subtitle { get; set; } = "";
    [Parameter, EditorRequired] public string AuthorName { get; set; } = "";
    [Parameter, EditorRequired] public string AuthorRole { get; set; } = "";
    [Parameter, EditorRequired] public string AuthorAvatarSrc { get; set; } = "";
}
