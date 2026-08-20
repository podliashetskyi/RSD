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

    [McpServerTool(Name = "create_content")]
    [Description("Create a content item. It ALWAYS lands as a Draft regardless of any status in the payload — publishing happens only via publish_content after review. Payload is the type's full JSON shape (see get_content on an existing item for reference; article body blocks use $type discriminators: richtext, subsection, bullets, quote, image, gallery, stats — and every block requires an \"id\" string (reuse ids from get_content; generate a fresh GUID for new blocks)). Returns the created item's id, slug, and status.")]
    public async Task<ContentListItem> CreateContentAsync(
        [Description("Content type key, e.g. \"blog\".")] string type,
        [Description("Full item payload as JSON.")] System.Text.Json.JsonElement payload,
        CancellationToken ct)
    {
        var descriptor = ContentTypeRegistry.Resolve(type);
        var id = await descriptor.CreateAsync(Services, payload, ct);
        return await SlimOfAsync(descriptor, id, ct);
    }

    [McpServerTool(Name = "update_content")]
    [Description("Update a content item by FULL REPLACEMENT: call get_content first and send the complete item back with your changes — omitted fields are treated as empty. The item's status never changes here. Editing a Published item is refused unless allowLiveEdit=true (get the user's explicit OK first: the change goes live immediately).")]
    public async Task<ContentListItem> UpdateContentAsync(
        [Description("Content type key, e.g. \"blog\".")] string type,
        [Description("The item's GUID id.")] string id,
        [Description("Complete item payload as JSON (full replacement).")] System.Text.Json.JsonElement payload,
        [Description("Must be true to edit a Published item; the edit goes live immediately.")] bool allowLiveEdit,
        CancellationToken ct)
    {
        if (!Guid.TryParse(id, out var itemId)) throw new McpException($"'{id}' is not a valid item id.");
        var descriptor = ContentTypeRegistry.Resolve(type);
        await descriptor.UpdateAsync(Services, itemId, payload, allowLiveEdit, ct);
        return await SlimOfAsync(descriptor, itemId, ct);
    }

    private async Task<ContentListItem> SlimOfAsync(ContentTypeRegistry.Descriptor descriptor, Guid id, CancellationToken ct)
    {
        var entity = (ContentEntity?)await descriptor.GetAsync(Services, id.ToString(), ct)
            ?? throw new McpException($"Item '{id}' vanished after the write.");
        return new ContentListItem(entity.Id, entity.Slug, descriptor.TitleOf(entity), entity.Status.ToString(), entity.UpdatedAt);
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
