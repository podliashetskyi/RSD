using RSD.Web.Data.Entities;
using RSD.Web.Services.Slugs;

namespace RSD.Web.Data.Seed;

public sealed class SocialLinkSeeder(AppDbContext Db, ISlugger Slugger) : SeederBase<SocialLink>(Db, Slugger)
{
    protected override Task<IReadOnlyList<SocialLink>> BuildAsync(CancellationToken ct)
    {
        IReadOnlyList<SocialLink> items =
        [
            // Footer (from Layout/Footer.razor) — Href intentionally empty;
            // Footer template skips rows without a real URL until an admin fills them in.
            Build(SocialLinkScope.Footer, "LinkedIn",  "images/icon-linkedin.svg",  "", 1),
            Build(SocialLinkScope.Footer, "X",         "images/icon-x.svg",         "", 2),
            Build(SocialLinkScope.Footer, "GitHub",    "images/icon-github.svg",    "", 3),
            Build(SocialLinkScope.Footer, "Facebook",  "images/icon-facebook.svg",  "", 4),
            Build(SocialLinkScope.Footer, "Instagram", "images/icon-instagram.svg", "", 5),
            // Contact (Sections/Contact/ContactSection)
            Build(SocialLinkScope.Contact, "LinkedIn", "images/contact/social/icon-linkedin.svg", "", 1),
            Build(SocialLinkScope.Contact, "Twitter",  "images/contact/social/icon-twitter.svg",  "", 2),
            Build(SocialLinkScope.Contact, "Reddit",   "images/contact/social/icon-reddit.svg",   "", 3),
            Build(SocialLinkScope.Contact, "Facebook", "images/contact/social/icon-facebook.svg", "", 4),
            // The Management scope used to live here; per-manager socials are now
            // edited on each TeamMember directly (BUG-009).
        ];
        return Task.FromResult(items);
    }

    private static SocialLink Build(SocialLinkScope scope, string label, string iconPath, string href, int order) => new()
    {
        Slug = $"{scope} {label}",
        Status = ContentStatus.Published,
        PublishedAt = DateTime.UtcNow,
        Label = label,
        IconPath = iconPath,
        Href = href,
        Scope = scope,
        DisplayOrder = order,
    };
}
