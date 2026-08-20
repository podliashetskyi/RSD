using ModelContextProtocol;
using RSD.Web.Data.Entities;
using RSD.Web.Services.Content;

namespace RSD.Web.Services.Mcp.Tools;

/// <summary>
/// Maps stable type keys to the content services behind them. All reads include drafts —
/// the agent must see unpublished work; the public site's own reads stay Published-only.
/// </summary>
internal static class ContentTypeRegistry
{
    internal sealed record Descriptor(
        Func<IServiceProvider, CancellationToken, Task<IReadOnlyList<ContentListItem>>> ListAsync,
        Func<IServiceProvider, string, CancellationToken, Task<object?>> GetAsync);

    private static readonly ContentQuery ReadQuery = new(PageSize: 200);

    private static readonly Dictionary<string, Descriptor> Types = new(StringComparer.OrdinalIgnoreCase)
    {
        ["blog"] = Main<BlogPost, BlogPostUpsert, IBlogService>(p => p.Title),
        ["cases"] = Main<Case, CaseUpsert, ICaseService>(c => c.Name),
        ["products"] = Main<Product, ProductUpsert, IProductService>(p => p.Name),
        ["services"] = Main<Service, ServiceUpsert, IServiceService>(s => s.Title),
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

    private static Descriptor Main<TEntity, TUpsert, TService>(Func<TEntity, string> title)
        where TEntity : ContentEntity
        where TService : class, IContentService<TEntity, TEntity, TUpsert>
        => new(
            async (sp, ct) => Slim(await sp.GetRequiredService<TService>().ListAsync(ReadQuery, ct), title),
            async (sp, key, ct) => Guid.TryParse(key, out var id)
                ? await sp.GetRequiredService<TService>().GetByIdAsync(id, ct)
                : await sp.GetRequiredService<TService>().GetBySlugAsync(key, includeDrafts: true, ct));

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
            });

    private static IReadOnlyList<ContentListItem> Slim<TEntity>(IReadOnlyList<TEntity> items, Func<TEntity, string> title)
        where TEntity : ContentEntity =>
        [.. items.OrderByDescending(e => e.UpdatedAt)
                 .Select(e => new ContentListItem(e.Id, e.Slug, title(e), e.Status.ToString(), e.UpdatedAt))];
}
