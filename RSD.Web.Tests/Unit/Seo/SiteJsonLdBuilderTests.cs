using System.Text.Json;
using FluentAssertions;
using RSD.Web.Services.Seo;

namespace RSD.Web.Tests.Unit.Seo;

/// <summary>
/// The sitewide JSON-LD graph is what lets AI engines recognize RSD as an entity.
/// These pin the graph shape, the stable @id anchors, and the script-safe serialization.
/// </summary>
public sealed class SiteJsonLdBuilderTests
{
    private const string Origin = "https://remsoft.dev";

    [Fact]
    public void Build_EmitsOrganizationAndWebSiteGraph_WithStableIds()
    {
        var json = SiteJsonLdBuilder.Build(Origin, ["https://www.linkedin.com/company/rsd"]);
        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        root.GetProperty("@context").GetString().Should().Be("https://schema.org");
        var graph = root.GetProperty("@graph");
        graph.GetArrayLength().Should().Be(2);

        var org = graph[0];
        org.GetProperty("@type").GetString().Should().Be("ProfessionalService");
        org.GetProperty("@id").GetString().Should().Be($"{Origin}#organization");
        org.GetProperty("name").GetString().Should().Be("RemSoft.Dev");
        org.GetProperty("url").GetString().Should().Be($"{Origin}/");
        org.GetProperty("logo").GetString().Should().Be($"{Origin}/images/logo.svg");
        org.GetProperty("inLanguage").GetString().Should().Be("en");
        org.GetProperty("knowsAbout").GetArrayLength().Should().BeGreaterThan(0);
        org.GetProperty("sameAs")[0].GetString().Should().Be("https://www.linkedin.com/company/rsd");

        var site = graph[1];
        site.GetProperty("@type").GetString().Should().Be("WebSite");
        site.GetProperty("@id").GetString().Should().Be($"{Origin}#website");
        site.GetProperty("publisher").GetProperty("@id").GetString().Should().Be($"{Origin}#organization");
    }

    [Fact]
    public void Build_OmitsSameAs_WhenNoSocialLinksExist()
    {
        var json = SiteJsonLdBuilder.Build(Origin, []);
        using var doc = JsonDocument.Parse(json);

        doc.RootElement.GetProperty("@graph")[0].TryGetProperty("sameAs", out _).Should().BeFalse();
    }

    [Fact]
    public void Build_NeverEmitsALiteralClosingScriptTag()
    {
        // Hrefs are admin-authored; a </script> inside the JSON would break out of the script element.
        var json = SiteJsonLdBuilder.Build(Origin, ["https://x.com/</script><script>alert(1)"]);

        json.Should().NotContain("</script>");
        json.Should().NotContain("<script>");
    }
}
