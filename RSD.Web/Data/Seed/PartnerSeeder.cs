using RSD.Web.Data.Entities;
using RSD.Web.Services.Slugs;

namespace RSD.Web.Data.Seed;

public sealed class PartnerSeeder(AppDbContext Db, ISlugger Slugger) : SeederBase<Partner>(Db, Slugger)
{
    protected override Task<IReadOnlyList<Partner>> BuildAsync(CancellationToken ct)
    {
        IReadOnlyList<Partner> items =
        [
            Build("Bonnie Green",  "Front-end Developer", "images/about/partners/portrait-bonnie-green.png",  "/contact", 1),
            Build("Robert Fox",    "Front-end Developer", "images/about/partners/portrait-bonnie-green.png",  "/contact", 2),
            Build("Eleanor Pena",  "Front-end Developer", "images/about/partners/portrait-eleanor-pena.png",  "/contact", 3),
            Build("Esther Howard", "Front-end Developer", "images/about/partners/portrait-esther-howard.png", "/contact", 4),
        ];
        return Task.FromResult(items);
    }

    private static Partner Build(string name, string role, string photo, string contactHref, int order) => new()
    {
        Slug = $"partner {name}",
        Status = ContentStatus.Published,
        PublishedAt = DateTime.UtcNow,
        Name = name,
        Role = role,
        PhotoPath = photo,
        ContactHref = contactHref,
        DisplayOrder = order,
    };
}
