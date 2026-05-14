#pragma warning disable S1144, S4487, S2933

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Http;
using RSD.Web.Components.Sections.Article;
using RSD.Web.Data.Entities;
using RSD.Web.Services.Content;

namespace RSD.Web.Components.Pages;

public partial class BlogDetail(
    IBlogService Blog,
    ITeamMemberService Team,
    IHttpContextAccessor Http)
{
    [Parameter] public string Slug { get; set; } = "";

    private BlogPost? Post { get; set; }
    private TeamMember? Author { get; set; }

    private static readonly IReadOnlyList<TocEntry> TocItems = [];

    private string HeroImage => string.IsNullOrEmpty(Post?.CoverImagePath) ? "images/services/cloud-solutions/hero.png" : Post!.CoverImagePath;
    private string DateText => (Post?.PublishedAt ?? Post?.CreatedAt ?? DateTime.UtcNow).ToString("MMMM dd, yyyy");
    private string ReadTimeText => Post is { ReadTimeMinutes: > 0 } ? $"{Post.ReadTimeMinutes} min" : "";
    private string AuthorName => Author?.Name ?? "RSD Team";
    private string AuthorRole => Author?.Role ?? "";
    private string AuthorAvatarSrc => Author?.AvatarPath ?? "images/avatars/avatar-default.png";

    protected override async Task OnInitializedAsync()
    {
        Post = await Blog.GetBySlugAsync(Slug, includeDrafts: false, CancellationToken.None);
        if (Post is null)
        {
            var http = Http.HttpContext;
            if (http is not null) http.Response.StatusCode = StatusCodes.Status404NotFound;
            return;
        }
        if (Post.AuthorId is { } authorId)
        {
            var team = await Team.ListAsync(new ContentQuery(Status: ContentStatus.Published, PageSize: 200), CancellationToken.None);
            Author = team.FirstOrDefault(t => t.Id == authorId);
        }
    }
}
