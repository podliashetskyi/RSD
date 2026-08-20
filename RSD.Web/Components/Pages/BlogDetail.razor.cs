#pragma warning disable S1144, S4487, S2933

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Http;
using RSD.Web.Components.Sections.Article;
using RSD.Web.Data.Entities;
using RSD.Web.Services.Content;
using RSD.Web.Services.Preview;
using RSD.Web.Services.Seo;

namespace RSD.Web.Components.Pages;

public partial class BlogDetail(
    IBlogService Blog,
    ITeamMemberService Team,
    IHttpContextAccessor Http,
    IPreviewContext PreviewCtx,
    PreviewLink Preview,
    Microsoft.Extensions.Options.IOptions<SeoOptions> Seo)
{
    private const string DefaultAuthorAvatarSrc = "images/logo.svg";

    [Parameter] public string Slug { get; set; } = "";
    [SupplyParameterFromQuery] public string? Token { get; set; }

    private BlogPost? Post { get; set; }
    private TeamMember? Author { get; set; }
    private IReadOnlyList<RSD.Web.Components.Sections.Shared.RelatedLink> Related { get; set; } = [];

    private static readonly IReadOnlyList<TocEntry> TocItems = [];

    private string HeroImage => string.IsNullOrEmpty(Post?.CoverImagePath) ? "images/services/cloud-solutions/hero.png" : Post!.CoverImagePath;
    private string HeroAlt => string.IsNullOrEmpty(Post?.CoverImageAlt) ? (Post?.Title ?? "") : Post!.CoverImageAlt;
    private DateTime PublishedOnValue => Post?.PublishedAt ?? Post?.CreatedAt ?? DateTime.UtcNow;
    private string DateText => PublishedOnValue.ToString("MMMM dd, yyyy");
    private string ReadTimeText => Post is { ReadTimeMinutes: > 0 } ? $"{Post.ReadTimeMinutes} min" : "";
    private string AuthorName => Author?.Name ?? "RSD Team";
    private string AuthorRole => Author?.Role ?? "";
    private string AuthorAvatarSrc => AvatarSrc(Author?.AvatarPath);

    private string SeoTitle => Post is null ? "" : SeoFallbacks.Title(Post.Seo, Post.Title);
    private string SeoDescription => Post is null ? "" : SeoFallbacks.Description(Post.Seo, Post.Summary, Post.Description);
    private string SeoOgImage => Post is null ? "" : SeoFallbacks.OgImage(Post.Seo, Post.CoverImagePath);
    private string SeoOgImageAlt => Post is null ? "" : SeoFallbacks.OgImageAlt(Post.Seo, Post.CoverImageAlt, Post.Title);
    private string SeoRobots => PreviewCtx.IsPreview ? "noindex" : "";
    private string PageJson => Post is null ? "" : PageJsonLdBuilder.BlogPosting(OriginValue, Post, Author);
    private string BreadcrumbJson => Post is null ? "" : BreadcrumbJsonLdBuilder.Build(OriginValue, "Blog", "/blog", Post.Title, $"/blog/{Post.Slug}");

    private string OriginValue =>
        Http.HttpContext is { } http ? RequestOrigin.Resolve(Seo.Value, http.Request) : new Origin(Seo.Value.BaseUrl).Value;

    protected override async Task OnInitializedAsync()
    {
        if (IsPreviewRequest() && !Preview.Verify("blog", Slug, Token))
        {
            NotFound();
            return;
        }
        PreviewCtx.IsPreview = IsPreviewRequest();

        Post = await Blog.GetBySlugAsync(Slug, includeDrafts: PreviewCtx.IsPreview, CancellationToken.None);
        if (Post is null)
        {
            NotFound();
            return;
        }
        if (Post.AuthorId is { } authorId)
        {
            var team = await Team.ListAsync(new ContentQuery(Status: ContentStatus.Published, PageSize: 200), CancellationToken.None);
            Author = team.FirstOrDefault(t => t.Id == authorId);
        }
        var pool = await Blog.ListAsync(new ContentQuery(Status: ContentStatus.Published, PageSize: 200), CancellationToken.None);
        Related = [.. RelatedSelector.Posts(Post, pool)
            .Select(p => new RSD.Web.Components.Sections.Shared.RelatedLink(p.Category, p.Title, $"/blog/{p.Slug}"))];
    }

    private bool IsPreviewRequest() =>
        Http.HttpContext?.Request.Path.StartsWithSegments("/preview") ?? false;

    private static string AvatarSrc(string? avatarPath) =>
        string.IsNullOrWhiteSpace(avatarPath) ? DefaultAuthorAvatarSrc : avatarPath;

    private void NotFound()
    {
        var http = Http.HttpContext;
        if (http is not null) http.Response.StatusCode = StatusCodes.Status404NotFound;
    }
}
