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
public sealed class ContentTools(IServiceProvider Services)
{
    [McpServerTool(Name = "list_content")]
    [Description("List content items of a type (drafts included). Types: blog, cases, products, services, testimonials, team, partners, values, stats, tech, contact-points, messenger-links, social-links, faq, terms-of-service, privacy-policy. Returns id, slug, title, status, updatedAt.")]
    public Task<IReadOnlyList<ContentListItem>> ListContentAsync(
        [Description("Content type key, e.g. \"blog\".")] string type,
        CancellationToken ct) =>
        ContentTypeRegistry.Resolve(type).ListAsync(Services, ct);

    [McpServerTool(Name = "get_content")]
    [Description("Get one content item in full (all fields, body JSON included), drafts included. Pass the item's slug or its id.")]
    public async Task<object> GetContentAsync(
        [Description("Content type key, e.g. \"blog\".")] string type,
        [Description("The item's slug, or its GUID id.")] string slugOrId,
        CancellationToken ct)
    {
        var found = await ContentTypeRegistry.Resolve(type).GetAsync(Services, slugOrId, ct);
        return found ?? throw new McpException($"No {type} item found for '{slugOrId}'.");
    }

    [McpServerTool(Name = "list_filters")]
    [Description("List the published taxonomy option labels for one filter type: BlogCategory, BlogTag, CaseIndustry, or CaseTechTag. Content tagging fields only accept these exact labels.")]
    public async Task<IReadOnlyList<string>> ListFiltersAsync(
        [Description("Filter type: BlogCategory | BlogTag | CaseIndustry | CaseTechTag")] string filterType,
        CancellationToken ct)
    {
        if (!Enum.TryParse<FilterType>(filterType, ignoreCase: true, out var parsed))
        {
            throw new McpException($"Unknown filter type '{filterType}'. Supported: {string.Join(", ", Enum.GetNames<FilterType>())}.");
        }
        var filters = await Services.GetRequiredService<IFilterService>().ListByTypeAsync(parsed, ct);
        return [.. filters.Select(f => f.Label)];
    }
}

/// <summary>Slim projection returned by list_content — keeps tool responses small.</summary>
public sealed record ContentListItem(Guid Id, string Slug, string Title, string Status, DateTime UpdatedAt);
