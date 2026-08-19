using FluentAssertions;
using RSD.Web.Data.Entities;
using RSD.Web.Services.Seo;

namespace RSD.Web.Tests.Unit.Seo;

/// <summary>
/// Detail pages resolve their head values through one fallback chain:
/// admin-entered SEO fields win, natural content fields fill the gaps.
/// </summary>
public sealed class SeoFallbacksTests
{
    [Fact]
    public void Title_PrefersMetaTitle_ThenNatural()
    {
        SeoFallbacks.Title(new SeoMetadata { MetaTitle = "Custom" }, "Natural").Should().Be("Custom");
        SeoFallbacks.Title(new SeoMetadata(), "Natural").Should().Be("Natural");
    }

    [Fact]
    public void Description_PrefersMetaDescription_ThenSummary_ThenDescription()
    {
        SeoFallbacks.Description(new SeoMetadata { MetaDescription = "M" }, "S", "D").Should().Be("M");
        SeoFallbacks.Description(new SeoMetadata(), "S", "D").Should().Be("S");
        SeoFallbacks.Description(new SeoMetadata(), "", "D").Should().Be("D");
        SeoFallbacks.Description(new SeoMetadata(), "", "").Should().Be("");
    }

    [Fact]
    public void OgImage_PrefersOgImagePath_ThenCover_ThenEmpty()
    {
        SeoFallbacks.OgImage(new SeoMetadata { OgImagePath = "uploads/seo/x.png" }, "images/c.png").Should().Be("uploads/seo/x.png");
        SeoFallbacks.OgImage(new SeoMetadata(), "images/c.png").Should().Be("images/c.png");
        SeoFallbacks.OgImage(new SeoMetadata(), "").Should().Be("");
    }

    [Fact]
    public void OgImageAlt_PrefersSeoAlt_ThenCoverAlt_ThenTitle()
    {
        SeoFallbacks.OgImageAlt(new SeoMetadata { OgImageAlt = "A" }, "CA", "T").Should().Be("A");
        SeoFallbacks.OgImageAlt(new SeoMetadata(), "CA", "T").Should().Be("CA");
        SeoFallbacks.OgImageAlt(new SeoMetadata(), "", "T").Should().Be("T");
    }
}
