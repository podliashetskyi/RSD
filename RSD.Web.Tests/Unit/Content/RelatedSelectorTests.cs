using FluentAssertions;
using RSD.Web.Data.Entities;
using RSD.Web.Services.Content;

namespace RSD.Web.Tests.Unit.Content;

/// <summary>
/// Related-content selection: taxonomy match (category/tag, industry/tech), never self,
/// newest first, capped at three.
/// </summary>
public sealed class RelatedSelectorTests
{
    private static BlogPost Post(string slug, string category, string[] tags, int daysAgo) => new()
    {
        Slug = slug, Title = slug, Category = category, Tags = [.. tags],
        PublishedAt = DateTime.UtcNow.AddDays(-daysAgo),
    };

    private static Case Case(string slug, string industry, string[] tech, int daysAgo) => new()
    {
        Slug = slug, Name = slug, Industry = industry, TechTags = [.. tech],
        PublishedAt = DateTime.UtcNow.AddDays(-daysAgo),
    };

    [Fact]
    public void RelatedPosts_MatchByCategoryOrSharedTag_ExcludeSelf_NewestFirst_CapThree()
    {
        var current = Post("current", "AI", ["dotnet"], 0);
        var pool = new List<BlogPost>
        {
            current,
            Post("same-cat-old", "AI", [], 30),
            Post("same-cat-new", "AI", [], 1),
            Post("shared-tag", "Cloud", ["dotnet"], 2),
            Post("unrelated", "Design", ["figma"], 3),
            Post("same-cat-mid", "AI", [], 10),
        };

        var related = RelatedSelector.Posts(current, pool);

        related.Select(p => p.Slug).Should().Equal("same-cat-new", "shared-tag", "same-cat-mid");
    }

    [Fact]
    public void RelatedCases_MatchByIndustryOrSharedTech_ExcludeSelf()
    {
        var current = Case("current", "Healthcare", ["React"], 0);
        var pool = new List<Case>
        {
            current,
            Case("same-industry", "Healthcare", [], 5),
            Case("shared-tech", "Fintech", ["React", "AWS"], 2),
            Case("unrelated", "Logistics", ["IoT"], 1),
        };

        var related = RelatedSelector.Cases(current, pool);

        related.Select(c => c.Slug).Should().Equal("shared-tech", "same-industry");
    }

    [Fact]
    public void RelatedPosts_NoMatches_ReturnsEmpty()
    {
        var current = Post("current", "AI", [], 0);
        RelatedSelector.Posts(current, [current, Post("other", "Design", ["figma"], 1)]).Should().BeEmpty();
    }
}
