using RSD.Web.Data.Entities;
using RSD.Web.Services.Slugs;

namespace RSD.Web.Data.Seed;

public sealed class ContactPointSeeder(AppDbContext Db, ISlugger Slugger) : SeederBase<ContactPoint>(Db, Slugger)
{
    protected override Task<IReadOnlyList<ContactPoint>> BuildAsync(CancellationToken ct)
    {
        IReadOnlyList<ContactPoint> items =
        [
            Build("Phone",   ["+1 (415) 555-1234"],                                       isLink: false, 1),
            Build("Email",   ["contactus@remsoft.dev"],                                  isLink: false, 2),
            Build("Address", ["San Francisco, CA 94102", "Business Center, Suite 100"],   isLink: true,  3),
        ];
        return Task.FromResult(items);
    }

    private static ContactPoint Build(string label, IReadOnlyList<string> lines, bool isLink, int order) => new()
    {
        Slug = label,
        Status = ContentStatus.Published,
        PublishedAt = DateTime.UtcNow,
        Label = label,
        Lines = [.. lines],
        IsLink = isLink,
        DisplayOrder = order,
    };
}
