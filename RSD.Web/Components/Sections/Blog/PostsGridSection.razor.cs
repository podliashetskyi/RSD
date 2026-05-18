#pragma warning disable S1144, S4487, S2933

using RSD.Web.Data.Entities;
using RSD.Web.Services.Content;

namespace RSD.Web.Components.Sections.Blog;

public partial class PostsGridSection(IBlogService Blog, ITeamMemberService Team, IFilterService Filters)
{
    private IReadOnlyList<BlogPostRow> Posts { get; set; } = [];
    private string Search { get; set; } = "";
    private string? Category { get; set; }

    private IReadOnlyList<string> Categories { get; set; } = [];

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
                    p.CardBlurb.Contains(s, StringComparison.OrdinalIgnoreCase));
            }
            return [.. q];
        }
    }

    protected override async Task OnInitializedAsync()
    {
        var posts = await Blog.ListAsync(new ContentQuery(Status: ContentStatus.Published, PageSize: 200), CancellationToken.None);
        var team = await Team.ListAsync(new ContentQuery(Status: ContentStatus.Published, PageSize: 200), CancellationToken.None);
        var categories = await Filters.ListByTypeAsync(FilterType.BlogCategory, CancellationToken.None);
        Posts = [.. posts.OrderByDescending(p => p.PublishedAt ?? p.CreatedAt).Select(p => BlogPostRow.From(p, team))];
        Categories = [.. categories.Select(f => f.Label)];
    }

    private void SetCategory(string? cat) => Category = cat;
}

public sealed record BlogPostRow(
    string Slug,
    string Title,
    string CardBlurb,
    string Category,
    IReadOnlyList<string> Tags,
    string CoverImagePath,
    string AuthorName,
    string AuthorAvatarSrc,
    string PublishedDate,
    string ReadTime)
{
    private const string DefaultAuthorAvatarSrc = "images/logo.svg";

    public static BlogPostRow From(BlogPost post, IReadOnlyList<TeamMember> team)
    {
        var author = post.AuthorId is { } authorId ? team.FirstOrDefault(t => t.Id == authorId) : null;
        return new BlogPostRow(
            post.Slug,
            post.Title,
            string.IsNullOrWhiteSpace(post.Summary) ? post.Description : post.Summary,
            post.Category,
            post.Tags,
            post.CoverImagePath,
            author?.Name ?? "RSD Team",
            AvatarSrc(author?.AvatarPath),
            (post.PublishedAt ?? post.CreatedAt).ToString("MMM dd, yyyy"),
            post.ReadTimeMinutes > 0 ? $"{post.ReadTimeMinutes} min read" : "");
    }

    private static string AvatarSrc(string? avatarPath) =>
        string.IsNullOrWhiteSpace(avatarPath) ? DefaultAuthorAvatarSrc : avatarPath;
}
