#pragma warning disable S1144, S4487, S2933

using RSD.Web.Data.Entities;
using RSD.Web.Services.Content;

namespace RSD.Web.Components.Sections.Blog;

public partial class PostsGridSection(IBlogService Blog, ITeamMemberService Team)
{
    private IReadOnlyList<BlogPostRow> Posts { get; set; } = [];
    private string Search { get; set; } = "";
    private string? Category { get; set; }

    private IReadOnlyList<string> Categories =>
        [.. Posts.Select(p => p.Category)
            .Where(s => !string.IsNullOrWhiteSpace(s))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(s => s)];

    private IReadOnlyList<BlogPostRow> DisplayedPosts
    {
        get
        {
            IEnumerable<BlogPostRow> q = Posts;
            if (Category is { } cat) q = q.Where(p => string.Equals(p.Category, cat, StringComparison.OrdinalIgnoreCase));
            if (!string.IsNullOrWhiteSpace(Search))
            {
                var s = Search.Trim();
                q = q.Where(p =>
                    p.Title.Contains(s, StringComparison.OrdinalIgnoreCase) ||
                    p.Description.Contains(s, StringComparison.OrdinalIgnoreCase));
            }
            return [.. q];
        }
    }

    protected override async Task OnInitializedAsync()
    {
        var posts = await Blog.ListAsync(new ContentQuery(Status: ContentStatus.Published, PageSize: 200), CancellationToken.None);
        var team = await Team.ListAsync(new ContentQuery(Status: ContentStatus.Published, PageSize: 200), CancellationToken.None);
        Posts = [.. posts.OrderByDescending(p => p.PublishedAt ?? p.CreatedAt).Select(p => BlogPostRow.From(p, team))];
    }

    private void SetCategory(string? cat) => Category = cat;
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
