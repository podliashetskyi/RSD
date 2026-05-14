#pragma warning disable S1144, S4487, S2933

using RSD.Web.Data.Entities;
using RSD.Web.Services.Content;

namespace RSD.Web.Components.Sections.Blog;

public partial class PostsGridSection(IBlogService Blog, ITeamMemberService Team)
{
    private IReadOnlyList<BlogPostRow> Posts { get; set; } = [];

    protected override async Task OnInitializedAsync()
    {
        var posts = await Blog.ListAsync(new ContentQuery(Status: ContentStatus.Published, PageSize: 200), CancellationToken.None);
        var team = await Team.ListAsync(new ContentQuery(Status: ContentStatus.Published, PageSize: 200), CancellationToken.None);
        Posts = [.. posts.OrderByDescending(p => p.PublishedAt ?? p.CreatedAt).Select(p => BlogPostRow.From(p, team))];
    }
}

public sealed record BlogPostRow(
    string Slug,
    string Title,
    string Description,
    string Category,
    IReadOnlyList<string> Tags,
    string CoverImagePath,
    string AuthorName,
    string AuthorAvatarSrc,
    string PublishedDate,
    string ReadTime)
{
    public static BlogPostRow From(BlogPost post, IReadOnlyList<TeamMember> team)
    {
        var author = post.AuthorId is { } authorId ? team.FirstOrDefault(t => t.Id == authorId) : null;
        return new BlogPostRow(
            post.Slug,
            post.Title,
            post.Description,
            post.Category,
            post.Tags,
            post.CoverImagePath,
            author?.Name ?? "RSD Team",
            author?.AvatarPath ?? "images/avatars/avatar-default.png",
            (post.PublishedAt ?? post.CreatedAt).ToString("MMM dd, yyyy"),
            post.ReadTimeMinutes > 0 ? $"{post.ReadTimeMinutes} min read" : "");
    }
}
