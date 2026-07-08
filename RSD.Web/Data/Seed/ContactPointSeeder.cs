using RSD.Web.Data.Entities;
using RSD.Web.Services.Slugs;

namespace RSD.Web.Data.Seed;

public sealed class ContactPointSeeder(AppDbContext Db, ISlugger Slugger) : SeederBase<ContactPoint>(Db, Slugger)
{
    protected override Task<IReadOnlyList<ContactPoint>> BuildAsync(CancellationToken ct)
    {
        IReadOnlyList<ContactPoint> items =
        [
            Build("Phone",   ["+1 (415) 555-1234"],                                      "tel:+14155551234",             "images/icon-phone.svg",    1),
            Build("Email",   ["contactus@remsoft.dev"],                                  "mailto:contactus@remsoft.dev", "images/icon-email.svg",    2),
            Build("Address", ["San Francisco, CA 94102", "Business Center, Suite 100"],  "",                             "images/icon-location.svg", 3),
        ];
        return Task.FromResult(items);
    }

    private static ContactPoint Build(string label, IReadOnlyList<string> lines, string href, string icon, int order) => new()
    {
        Slug = label,
        Status = ContentStatus.Published,
        PublishedAt = DateTime.UtcNow,
        Label = label,
        Lines = [.. lines],
        Href = href,
        IconPath = icon,
        IsLink = !string.IsNullOrWhiteSpace(href),
        DisplayOrder = order,
    };
}
