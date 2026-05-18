using Microsoft.EntityFrameworkCore;
using RSD.Web.Data.Entities;

namespace RSD.Web.Data.Seed;

public sealed class PublicPlaceholderCleanupSeeder(AppDbContext Db) : ISeeder
{
    private const string PublicEmail = "contactus@remsoft.dev";

    private static readonly HashSet<string> DummyUrls = new(StringComparer.OrdinalIgnoreCase)
    {
        "#",
        "google.com",
        "https://google.com",
        "https://google.com/",
        "https://www.google.com",
        "https://www.google.com/",
        "https://www.linkedin.com",
        "https://www.linkedin.com/",
        "https://x.com",
        "https://x.com/",
        "https://github.com",
        "https://github.com/",
    };

    public async Task SeedAsync(CancellationToken ct)
    {
        var changed = false;
        changed |= await CleanContactPointsAsync(ct);
        changed |= await CleanTeamMembersAsync(ct);
        changed |= await CleanSocialLinksAsync(ct);
        changed |= await CleanMessengerLinksAsync(ct);
        changed |= await CleanLegalPagesAsync(ct);
        if (changed) await Db.SaveChangesAsync(ct);
    }

    private async Task<bool> CleanContactPointsAsync(CancellationToken ct)
    {
        var contacts = await Db.ContactPoints.ToListAsync(ct);
        return contacts.Aggregate(false, (changed, point) => CleanContactPoint(point) || changed);
    }

    private static bool CleanContactPoint(ContactPoint point)
    {
        var cleaned = point.Lines.Select(CleanPublicText).ToList();
        if (point.Lines.SequenceEqual(cleaned)) return false;
        point.Lines = cleaned;
        return true;
    }

    private async Task<bool> CleanTeamMembersAsync(CancellationToken ct)
    {
        var members = await Db.TeamMembers.ToListAsync(ct);
        return members.Aggregate(false, (changed, member) => CleanTeamMember(member) || changed);
    }

    private static bool CleanTeamMember(TeamMember member)
    {
        var changed = false;
        changed |= ClearDummyUrl(value => member.LinkedInUrl = value, member.LinkedInUrl);
        changed |= ClearDummyUrl(value => member.XUrl = value, member.XUrl);
        changed |= ClearDummyUrl(value => member.GitHubUrl = value, member.GitHubUrl);
        changed |= ClearExampleEmail(member);
        return changed;
    }

    private async Task<bool> CleanSocialLinksAsync(CancellationToken ct)
    {
        var links = await Db.SocialLinks.ToListAsync(ct);
        return links.Aggregate(false, (changed, link) => ClearDummyUrl(value => link.Href = value, link.Href) || changed);
    }

    private async Task<bool> CleanMessengerLinksAsync(CancellationToken ct)
    {
        var links = await Db.MessengerLinks.ToListAsync(ct);
        return links.Aggregate(false, (changed, link) => ClearDummyUrl(value => link.Href = value, link.Href) || changed);
    }

    private async Task<bool> CleanLegalPagesAsync(CancellationToken ct)
    {
        var terms = await Db.TermsOfService.ToListAsync(ct);
        var policies = await Db.PrivacyPolicies.ToListAsync(ct);
        return CleanTerms(terms) | CleanPolicies(policies);
    }

    private static bool CleanTerms(IEnumerable<TermsOfService> terms) =>
        terms.Aggregate(false, (changed, page) => CleanLegalBody(page) || changed);

    private static bool CleanPolicies(IEnumerable<PrivacyPolicy> policies) =>
        policies.Aggregate(false, (changed, page) => CleanLegalBody(page) || changed);

    private static bool CleanLegalBody(TermsOfService page)
    {
        var cleaned = CleanPublicText(page.BodyHtml);
        if (page.BodyHtml == cleaned) return false;
        page.BodyHtml = cleaned;
        return true;
    }

    private static bool CleanLegalBody(PrivacyPolicy page)
    {
        var cleaned = CleanPublicText(page.BodyHtml);
        if (page.BodyHtml == cleaned) return false;
        page.BodyHtml = cleaned;
        return true;
    }

    private static bool ClearDummyUrl(Action<string> setValue, string value)
    {
        if (!DummyUrls.Contains(value.Trim())) return false;
        setValue("");
        return true;
    }

    private static bool ClearExampleEmail(TeamMember member)
    {
        if (!member.Email.EndsWith("@example.com", StringComparison.OrdinalIgnoreCase)) return false;
        member.Email = "";
        return true;
    }

    private static string CleanPublicText(string value) =>
        value.Replace("hello@nexatech.io", PublicEmail, StringComparison.OrdinalIgnoreCase)
             .Replace("hello@remsoft.dev", PublicEmail, StringComparison.OrdinalIgnoreCase);
}
