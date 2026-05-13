using FluentAssertions;
using RSD.Web.Data.Entities;

namespace RSD.Web.Tests.Unit.Entities;

public sealed class ContentEntityDefaultsTests
{
    private sealed record class TestEntity : ContentEntity;

    [Fact]
    public void IsDeleted_DefaultsToFalse()
    {
        new TestEntity { Slug = "x" }.IsDeleted.Should().BeFalse();
    }

    [Fact]
    public void Status_DefaultsToDraft()
    {
        new TestEntity { Slug = "x" }.Status.Should().Be(ContentStatus.Draft);
    }

    [Fact]
    public void CreatedAt_AndUpdatedAt_PopulatedAtConstruction()
    {
        var before = DateTime.UtcNow.AddSeconds(-1);
        var after = DateTime.UtcNow.AddSeconds(1);
        var e = new TestEntity { Slug = "x" };
        e.CreatedAt.Should().BeOnOrAfter(before).And.BeOnOrBefore(after);
        e.UpdatedAt.Should().BeOnOrAfter(before).And.BeOnOrBefore(after);
    }

    [Fact]
    public void Id_HasNonEmptyDefault()
    {
        new TestEntity { Slug = "x" }.Id.Should().NotBe(Guid.Empty);
    }

    [Fact]
    public void Seo_IsNonNull_AndHasEmptyDefaults()
    {
        var seo = new TestEntity { Slug = "x" }.Seo;
        seo.Should().NotBeNull();
        seo.MetaTitle.Should().BeEmpty();
        seo.MetaDescription.Should().BeEmpty();
        seo.OgImagePath.Should().BeEmpty();
    }

    [Fact]
    public void PublishedAt_DefaultsToNull()
    {
        new TestEntity { Slug = "x" }.PublishedAt.Should().BeNull();
    }
}
