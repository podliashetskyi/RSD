using Microsoft.EntityFrameworkCore;
using RSD.Web.Data;
using RSD.Web.Services.Common;

namespace RSD.Web.Services.Content.Trash;

public sealed class TrashService(
    IDbContextFactory<AppDbContext> DbFactory,
    IBlogService Blog,
    ICaseService Cases,
    IProductService Products,
    IServiceService Services,
    ITestimonialService Testimonials,
    ITeamMemberService Team,
    IPartnerService Partners,
    IValueService Values,
    IMissionStatService MissionStats,
    ITechStackItemService TechStack,
    IContactPointService ContactPoints,
    IMessengerLinkService Messengers,
    ISocialLinkService Socials,
    IFaqItemService FaqItems) : ITrashService
{
    public async Task<IReadOnlyList<TrashItem>> ListAsync(CancellationToken ct)
    {
        await using var db = await DbFactory.CreateDbContextAsync(ct);
        var items = new List<TrashItem>();
        items.AddRange(await db.BlogPosts.IgnoreQueryFilters().Where(e => e.IsDeleted)
            .Select(e => new TrashItem("blog", "Blog post", e.Id, e.Title, e.Slug, e.UpdatedAt)).ToListAsync(ct));
        items.AddRange(await db.Cases.IgnoreQueryFilters().Where(e => e.IsDeleted)
            .Select(e => new TrashItem("cases", "Case", e.Id, e.Name, e.Slug, e.UpdatedAt)).ToListAsync(ct));
        items.AddRange(await db.Products.IgnoreQueryFilters().Where(e => e.IsDeleted)
            .Select(e => new TrashItem("products", "Product", e.Id, e.Name, e.Slug, e.UpdatedAt)).ToListAsync(ct));
        items.AddRange(await db.Services.IgnoreQueryFilters().Where(e => e.IsDeleted)
            .Select(e => new TrashItem("services", "Service", e.Id, e.Title, e.Slug, e.UpdatedAt)).ToListAsync(ct));
        items.AddRange(await db.Testimonials.IgnoreQueryFilters().Where(e => e.IsDeleted)
            .Select(e => new TrashItem("testimonials", "Testimonial", e.Id, e.Title, e.Slug, e.UpdatedAt)).ToListAsync(ct));
        items.AddRange(await db.TeamMembers.IgnoreQueryFilters().Where(e => e.IsDeleted)
            .Select(e => new TrashItem("team", "Team member", e.Id, e.Name, e.Slug, e.UpdatedAt)).ToListAsync(ct));
        items.AddRange(await db.Partners.IgnoreQueryFilters().Where(e => e.IsDeleted)
            .Select(e => new TrashItem("partners", "Partner", e.Id, e.Name, e.Slug, e.UpdatedAt)).ToListAsync(ct));
        items.AddRange(await db.Values.IgnoreQueryFilters().Where(e => e.IsDeleted)
            .Select(e => new TrashItem("values", "Value", e.Id, e.Title, e.Slug, e.UpdatedAt)).ToListAsync(ct));
        items.AddRange(await db.MissionStats.IgnoreQueryFilters().Where(e => e.IsDeleted)
            .Select(e => new TrashItem("stats", "Mission stat", e.Id, e.Label, e.Slug, e.UpdatedAt)).ToListAsync(ct));
        items.AddRange(await db.TechStackItems.IgnoreQueryFilters().Where(e => e.IsDeleted)
            .Select(e => new TrashItem("tech", "Tech stack item", e.Id, e.Label, e.Slug, e.UpdatedAt)).ToListAsync(ct));
        items.AddRange(await db.ContactPoints.IgnoreQueryFilters().Where(e => e.IsDeleted)
            .Select(e => new TrashItem("contact-points", "Contact point", e.Id, e.Label, e.Slug, e.UpdatedAt)).ToListAsync(ct));
        items.AddRange(await db.MessengerLinks.IgnoreQueryFilters().Where(e => e.IsDeleted)
            .Select(e => new TrashItem("messenger-links", "Messenger link", e.Id, e.Label, e.Slug, e.UpdatedAt)).ToListAsync(ct));
        items.AddRange(await db.SocialLinks.IgnoreQueryFilters().Where(e => e.IsDeleted)
            .Select(e => new TrashItem("social-links", "Social link", e.Id, e.Label, e.Slug, e.UpdatedAt)).ToListAsync(ct));
        items.AddRange(await db.FaqItems.IgnoreQueryFilters().Where(e => e.IsDeleted)
            .Select(e => new TrashItem("faq", "FAQ item", e.Id, e.Question, e.Slug, e.UpdatedAt)).ToListAsync(ct));
        return [.. items.OrderByDescending(i => i.DeletedAt)];
    }

    public Task<Result<Unit>> RestoreAsync(string entityKey, Guid id, CancellationToken ct) =>
        Dispatch(entityKey, id, ct, restore: true);

    public Task<Result<Unit>> HardDeleteAsync(string entityKey, Guid id, CancellationToken ct) =>
        Dispatch(entityKey, id, ct, restore: false);

    private Task<Result<Unit>> Dispatch(string entityKey, Guid id, CancellationToken ct, bool restore) => entityKey switch
    {
        "blog" => Apply(Blog.RestoreAsync, Blog.HardDeleteAsync, id, ct, restore),
        "cases" => Apply(Cases.RestoreAsync, Cases.HardDeleteAsync, id, ct, restore),
        "products" => Apply(Products.RestoreAsync, Products.HardDeleteAsync, id, ct, restore),
        "services" => Apply(Services.RestoreAsync, Services.HardDeleteAsync, id, ct, restore),
        "testimonials" => Apply(Testimonials.RestoreAsync, Testimonials.HardDeleteAsync, id, ct, restore),
        "team" => Apply(Team.RestoreAsync, Team.HardDeleteAsync, id, ct, restore),
        "partners" => Apply(Partners.RestoreAsync, Partners.HardDeleteAsync, id, ct, restore),
        "values" => Apply(Values.RestoreAsync, Values.HardDeleteAsync, id, ct, restore),
        "stats" => Apply(MissionStats.RestoreAsync, MissionStats.HardDeleteAsync, id, ct, restore),
        "tech" => Apply(TechStack.RestoreAsync, TechStack.HardDeleteAsync, id, ct, restore),
        "contact-points" => Apply(ContactPoints.RestoreAsync, ContactPoints.HardDeleteAsync, id, ct, restore),
        "messenger-links" => Apply(Messengers.RestoreAsync, Messengers.HardDeleteAsync, id, ct, restore),
        "social-links" => Apply(Socials.RestoreAsync, Socials.HardDeleteAsync, id, ct, restore),
        "faq" => Apply(FaqItems.RestoreAsync, FaqItems.HardDeleteAsync, id, ct, restore),
        _ => Task.FromResult(Result.Fail($"Unknown entity type '{entityKey}'.")),
    };

    private static Task<Result<Unit>> Apply(
        Func<Guid, CancellationToken, Task<Result<Unit>>> restore,
        Func<Guid, CancellationToken, Task<Result<Unit>>> hardDelete,
        Guid id, CancellationToken ct, bool restoreOp) =>
        restoreOp ? restore(id, ct) : hardDelete(id, ct);
}
