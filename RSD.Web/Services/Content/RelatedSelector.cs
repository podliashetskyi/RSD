using RSD.Web.Data.Entities;

namespace RSD.Web.Services.Content;

/// <summary>
/// Related-content selection for detail pages: taxonomy match, never the current item,
/// newest first, capped at three.
/// </summary>
internal static class RelatedSelector
{
    private const int Cap = 3;

    internal static IReadOnlyList<BlogPost> Posts(BlogPost current, IReadOnlyList<BlogPost> pool) =>
        [.. pool.Where(p => p.Id != current.Id && MatchesPost(current, p))
                .OrderByDescending(p => p.PublishedAt ?? p.CreatedAt)
                .Take(Cap)];

    internal static IReadOnlyList<Case> Cases(Case current, IReadOnlyList<Case> pool) =>
        [.. pool.Where(c => c.Id != current.Id && MatchesCase(current, c))
                .OrderByDescending(c => c.PublishedAt ?? c.CreatedAt)
                .Take(Cap)];

    private static bool MatchesPost(BlogPost current, BlogPost candidate) =>
        SameText(current.Category, candidate.Category) || SharesAny(current.Tags, candidate.Tags);

    private static bool MatchesCase(Case current, Case candidate) =>
        SameText(current.Industry, candidate.Industry) || SharesAny(current.TechTags, candidate.TechTags);

    private static bool SameText(string a, string b) =>
        a.Length > 0 && string.Equals(a, b, StringComparison.OrdinalIgnoreCase);

    private static bool SharesAny(IReadOnlyList<string> a, IReadOnlyList<string> b) =>
        a.Intersect(b, StringComparer.OrdinalIgnoreCase).Any();
}
