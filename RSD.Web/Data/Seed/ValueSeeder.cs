using RSD.Web.Data.Entities;
using RSD.Web.Services.Slugs;

namespace RSD.Web.Data.Seed;

public sealed class ValueSeeder(AppDbContext Db, ISlugger Slugger) : SeederBase<Value>(Db, Slugger)
{
    protected override Task<IReadOnlyList<Value>> BuildAsync(CancellationToken ct)
    {
        IReadOnlyList<Value> items =
        [
            Build("Results",     "We focus on outcomes for clients, not just tasks.",                       "images/about/values/icon-rocket.svg",    1),
            Build("Innovation",  "Exploring new technologies and best practices.",                          "images/about/values/icon-lightbulb.svg", 2),
            Build("Partnership", "Building long-term relationships based on trust and mutual respect.",     "images/about/values/icon-heart.svg",     3),
            Build("Team",        "Investing in our team's development — our main asset.",                   "images/about/values/icon-users.svg",     4),
        ];
        return Task.FromResult(items);
    }

    private static Value Build(string title, string description, string icon, int order) => new()
    {
        Slug = title,
        Status = ContentStatus.Published,
        PublishedAt = DateTime.UtcNow,
        Title = title,
        Description = description,
        IconPath = icon,
        DisplayOrder = order,
    };
}
