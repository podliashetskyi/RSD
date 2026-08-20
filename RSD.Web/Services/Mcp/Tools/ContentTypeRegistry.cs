using System.Text.Json;
using ModelContextProtocol;
using RSD.Web.Data.Entities;
using RSD.Web.Services.Content;

namespace RSD.Web.Services.Mcp.Tools;

/// <summary>
/// Maps stable type keys to the content services behind them. All reads include drafts —
/// the agent must see unpublished work; the public site's own reads stay Published-only.
/// Writes: create always lands as Draft; update pins the item's existing status.
/// </summary>
internal static class ContentTypeRegistry
{
    internal sealed record Descriptor(
        Func<IServiceProvider, CancellationToken, Task<IReadOnlyList<ContentListItem>>> ListAsync,
        Func<IServiceProvider, string, CancellationToken, Task<object?>> GetAsync,
        Func<object, string> TitleOf,
        Func<IServiceProvider, JsonElement, CancellationToken, Task<Guid>> CreateAsync,
        Func<IServiceProvider, Guid, JsonElement, bool, CancellationToken, Task> UpdateAsync);

    private static readonly ContentQuery ReadQuery = new(PageSize: 200);

    private static readonly Dictionary<string, Descriptor> Types = new(StringComparer.OrdinalIgnoreCase)
    {
        ["blog"] = Main<BlogPost, BlogPostUpsert, IBlogService>(p => p.Title, FinalizeBlog),
        ["cases"] = Main<Case, CaseUpsert, ICaseService>(c => c.Name, FinalizeCase),
        ["products"] = Main<Product, ProductUpsert, IProductService>(p => p.Name, FinalizeProduct),
        ["services"] = Main<Service, ServiceUpsert, IServiceService>(s => s.Title, FinalizeService),
        ["testimonials"] = Simple<Testimonial, ITestimonialService>(t => t.Title),
        ["team"] = Simple<TeamMember, ITeamMemberService>(t => t.Name),
        ["partners"] = Simple<Partner, IPartnerService>(p => p.Name),
        ["values"] = Simple<Value, IValueService>(v => v.Title),
        ["stats"] = Simple<MissionStat, IMissionStatService>(s => s.Label),
        ["tech"] = Simple<TechStackItem, ITechStackItemService>(t => t.Label),
        ["contact-points"] = Simple<ContactPoint, IContactPointService>(c => c.Label),
        ["messenger-links"] = Simple<MessengerLink, IMessengerLinkService>(m => m.Label),
        ["social-links"] = Simple<SocialLink, ISocialLinkService>(s => s.Label),
        ["faq"] = Simple<FaqItem, IFaqItemService>(f => f.Question),
        ["terms-of-service"] = Simple<TermsOfService, ITermsOfServiceService>(t => t.Title),
        ["privacy-policy"] = Simple<PrivacyPolicy, IPrivacyPolicyService>(p => p.Title),
    };

    internal static IReadOnlyCollection<string> Keys => Types.Keys;

    internal static Descriptor Resolve(string type) =>
        Types.TryGetValue(type, out var descriptor)
            ? descriptor
            : throw new McpException($"Unknown content type '{type}'. Supported: {string.Join(", ", Types.Keys.OrderBy(k => k))}.");

    private static Descriptor Main<TEntity, TUpsert, TService>(
        Func<TEntity, string> title,
        Func<TUpsert, ContentStatus, TUpsert> finalize)
        where TEntity : ContentEntity
        where TService : class, IContentService<TEntity, TEntity, TUpsert>
        => new(
            async (sp, ct) => Slim(await sp.GetRequiredService<TService>().ListAsync(ReadQuery, ct), title),
            async (sp, key, ct) => Guid.TryParse(key, out var id)
                ? await sp.GetRequiredService<TService>().GetByIdAsync(id, ct)
                : await sp.GetRequiredService<TService>().GetBySlugAsync(key, includeDrafts: true, ct),
            e => title((TEntity)e),
            async (sp, payload, ct) =>
            {
                var upsert = finalize(McpJson.Deserialize<TUpsert>(payload, typeof(TEntity).Name), ContentStatus.Draft);
                var result = await sp.GetRequiredService<TService>().CreateAsync(upsert, ct);
                return result.Ok ? result.Value : throw new McpException(result.Error);
            },
            async (sp, id, payload, allowLiveEdit, ct) =>
            {
                var service = sp.GetRequiredService<TService>();
                var existing = await service.GetByIdAsync(id, ct)
                    ?? throw new McpException($"No item with id '{id}'.");
                GuardLiveEdit(existing.Status, allowLiveEdit);
                var upsert = finalize(McpJson.Deserialize<TUpsert>(payload, typeof(TEntity).Name), existing.Status);
                var result = await service.UpdateAsync(id, upsert, ct);
                if (!result.Ok) throw new McpException(result.Error);
            });

    private static Descriptor Simple<TEntity, TService>(Func<TEntity, string> title)
        where TEntity : ContentEntity
        where TService : class, ISimpleContentService<TEntity>
        => new(
            async (sp, ct) => Slim(await sp.GetRequiredService<TService>().ListAsync(ReadQuery, ct), title),
            async (sp, key, ct) =>
            {
                var service = sp.GetRequiredService<TService>();
                if (Guid.TryParse(key, out var id)) return await service.GetByIdAsync(id, ct);
                var all = await service.ListAsync(ReadQuery, ct);
                return all.FirstOrDefault(e => string.Equals(e.Slug, key, StringComparison.OrdinalIgnoreCase));
            },
            e => title((TEntity)e),
            async (sp, payload, ct) =>
            {
                var entity = McpJson.Deserialize<TEntity>(payload, typeof(TEntity).Name);
                entity.Status = ContentStatus.Draft;
                var result = await sp.GetRequiredService<TService>().CreateAsync(entity, ct);
                return result.Ok ? result.Value : throw new McpException(result.Error);
            },
            async (sp, id, payload, allowLiveEdit, ct) =>
            {
                var service = sp.GetRequiredService<TService>();
                var existing = await service.GetByIdAsync(id, ct)
                    ?? throw new McpException($"No item with id '{id}'.");
                GuardLiveEdit(existing.Status, allowLiveEdit);
                var entity = McpJson.Deserialize<TEntity>(payload, typeof(TEntity).Name);
                if (entity.Id != id)
                {
                    throw new McpException(
                        "The payload's id must equal the target id — echo the item from get_content and edit that (full replacement).");
                }
                entity.Status = existing.Status;
                var result = await service.UpdateAsync(entity, ct);
                if (!result.Ok) throw new McpException(result.Error);
            });

    private static void GuardLiveEdit(ContentStatus status, bool allowLiveEdit)
    {
        if (status == ContentStatus.Published && !allowLiveEdit)
        {
            throw new McpException(
                "This item is Published — an update would change the live site immediately. " +
                "Get the user's explicit OK, then retry with allowLiveEdit=true.");
        }
    }

    private static IReadOnlyList<ContentListItem> Slim<TEntity>(IReadOnlyList<TEntity> items, Func<TEntity, string> title)
        where TEntity : ContentEntity =>
        [.. items.OrderByDescending(e => e.UpdatedAt)
                 .Select(e => new ContentListItem(e.Id, e.Slug, title(e), e.Status.ToString(), e.UpdatedAt))];

    // Finalizers scrub nulls from partial payloads (missing JSON members deserialize to null)
    // and pin the status decided by the guardrails — never the payload's own status.
    private static BlogPostUpsert FinalizeBlog(BlogPostUpsert u, ContentStatus status) => u with
    {
        Slug = u.Slug ?? "", Title = u.Title ?? "", Summary = u.Summary ?? "", Description = u.Description ?? "",
        Category = u.Category ?? "", CoverImagePath = u.CoverImagePath ?? "", CoverImageAlt = u.CoverImageAlt ?? "",
        Tags = u.Tags ?? [], Intro = u.Intro ?? "", Status = status, Seo = u.Seo ?? new(), Body = u.Body ?? new(),
    };

    private static CaseUpsert FinalizeCase(CaseUpsert u, ContentStatus status) => u with
    {
        Slug = u.Slug ?? "", Name = u.Name ?? "", Summary = u.Summary ?? "", Industry = u.Industry ?? "",
        Description = u.Description ?? "", CoverImagePath = u.CoverImagePath ?? "", CoverImageAlt = u.CoverImageAlt ?? "",
        TechTags = u.TechTags ?? [], Status = status, Seo = u.Seo ?? new(), DetailFields = u.DetailFields ?? new(),
    };

    private static ProductUpsert FinalizeProduct(ProductUpsert u, ContentStatus status) => u with
    {
        Slug = u.Slug ?? "", Name = u.Name ?? "", Summary = u.Summary ?? "", Subtitle = u.Subtitle ?? "",
        Price = u.Price ?? "", Description = u.Description ?? "", BulletPoints = u.BulletPoints ?? [],
        CoverImagePath = u.CoverImagePath ?? "", CoverImageAlt = u.CoverImageAlt ?? "",
        TryForFreeHref = u.TryForFreeHref ?? "", LearnMoreHref = u.LearnMoreHref ?? "",
        Status = status, Seo = u.Seo ?? new(), DetailFields = u.DetailFields ?? new(),
    };

    private static ServiceUpsert FinalizeService(ServiceUpsert u, ContentStatus status) => u with
    {
        Slug = u.Slug ?? "", Title = u.Title ?? "", Summary = u.Summary ?? "", Description = u.Description ?? "",
        BulletPoints = u.BulletPoints ?? [], CoverImagePath = u.CoverImagePath ?? "", CoverImageAlt = u.CoverImageAlt ?? "",
        DetailsHref = u.DetailsHref ?? "", Intro = u.Intro ?? "", Status = status, Seo = u.Seo ?? new(), Body = u.Body ?? new(),
    };
}
