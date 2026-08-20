using System.ComponentModel;
using ModelContextProtocol;
using ModelContextProtocol.Server;
using RSD.Web.Data.Entities;
using RSD.Web.Services.Content;

namespace RSD.Web.Services.Mcp.Tools;

/// <summary>
/// Content-editing tools exposed to the local AI agent. Every tool calls the existing
/// content services — never the DbContext — so sanitization, slugs, refcounts, cache
/// eviction, and audit all apply exactly as they do for the admin UI.
/// </summary>
[McpServerToolType]
public sealed class ContentTools(IBlogService Blog)
{
    [McpServerTool(Name = "list_content")]
    [Description("List content items of a type. Supported types: blog. Returns id, slug, title, status, and updatedAt for each item, drafts included.")]
    public async Task<IReadOnlyList<ContentListItem>> ListContentAsync(
        [Description("Content type key, e.g. \"blog\".")] string type,
        CancellationToken ct)
    {
        if (!string.Equals(type, "blog", StringComparison.OrdinalIgnoreCase))
        {
            throw new McpException($"Unknown content type '{type}'. Supported: blog.");
        }
        var posts = await Blog.ListAsync(new ContentQuery(PageSize: 200), ct);
        return [.. posts.OrderByDescending(p => p.UpdatedAt).Select(ContentListItem.From)];
    }
}

/// <summary>Slim projection returned by list_content — keeps tool responses small.</summary>
public sealed record ContentListItem(Guid Id, string Slug, string Title, string Status, DateTime UpdatedAt)
{
    internal static ContentListItem From(BlogPost p) => new(p.Id, p.Slug, p.Title, p.Status.ToString(), p.UpdatedAt);
}
