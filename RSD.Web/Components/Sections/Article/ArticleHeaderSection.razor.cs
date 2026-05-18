#pragma warning disable S1144, S4487, S2933
using Microsoft.AspNetCore.Components;

namespace RSD.Web.Components.Sections.Article;

public partial class ArticleHeaderSection(NavigationManager Nav)
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

    private string TwitterShareHref => $"https://twitter.com/intent/tweet?url={EncodedPageUrl}&text={EncodedTitle}";
    private string LinkedInShareHref => $"https://www.linkedin.com/sharing/share-offsite/?url={EncodedPageUrl}";
    private string FacebookShareHref => $"https://www.facebook.com/sharer/sharer.php?u={EncodedPageUrl}";
    private string EncodedPageUrl => Uri.EscapeDataString(Nav.Uri);
    private string EncodedTitle => Uri.EscapeDataString(Title);
}
