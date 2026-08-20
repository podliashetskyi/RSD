using System.Text.Json;
using FluentAssertions;
using RSD.Web.Services.Seo;

namespace RSD.Web.Tests.Unit.Seo;

/// <summary>
/// BreadcrumbList schema (schema-only by design decision — no visible trail):
/// Home → Section → Item with absolute URLs and 1-based positions.
/// </summary>
public sealed class BreadcrumbJsonLdBuilderTests
{
    [Fact]
    public void Build_EmitsThreeLevelTrail_WithAbsoluteUrls()
    {
        var json = BreadcrumbJsonLdBuilder.Build(
            "https://remsoft.dev", "Cases", "/cases", "Industrial AI", "/cases/industrial-ai");
        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        root.GetProperty("@type").GetString().Should().Be("BreadcrumbList");
        var items = root.GetProperty("itemListElement");
        items.GetArrayLength().Should().Be(3);

        items[0].GetProperty("position").GetInt32().Should().Be(1);
        items[0].GetProperty("name").GetString().Should().Be("Home");
        items[0].GetProperty("item").GetString().Should().Be("https://remsoft.dev/");

        items[1].GetProperty("position").GetInt32().Should().Be(2);
        items[1].GetProperty("name").GetString().Should().Be("Cases");
        items[1].GetProperty("item").GetString().Should().Be("https://remsoft.dev/cases");

        items[2].GetProperty("position").GetInt32().Should().Be(3);
        items[2].GetProperty("name").GetString().Should().Be("Industrial AI");
        items[2].GetProperty("item").GetString().Should().Be("https://remsoft.dev/cases/industrial-ai");
        items[2].GetProperty("@type").GetString().Should().Be("ListItem");
    }
}
