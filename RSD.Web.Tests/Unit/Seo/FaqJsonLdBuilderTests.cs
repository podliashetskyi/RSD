using System.Text.Json;
using FluentAssertions;
using RSD.Web.Data.Entities;
using RSD.Web.Services.Seo;

namespace RSD.Web.Tests.Unit.Seo;

/// <summary>
/// FAQPage schema must mirror the visibly rendered Q&amp;A pairs exactly (parity rule) —
/// engines cross-check structured data against on-page text.
/// </summary>
public sealed class FaqJsonLdBuilderTests
{
    [Fact]
    public void Build_EmitsFaqPage_WithQuestionAndAnswerPairs()
    {
        var items = new List<FaqItem>
        {
            new() { Slug = "a", Question = "Do you sign NDAs?", AnswerHtml = "<p>Yes, before details are shared.</p>" },
            new() { Slug = "b", Question = "Who owns the code?", AnswerHtml = "<p>You do.</p>" },
        };

        var json = FaqJsonLdBuilder.Build(items);
        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        root.GetProperty("@type").GetString().Should().Be("FAQPage");
        var main = root.GetProperty("mainEntity");
        main.GetArrayLength().Should().Be(2);
        main[0].GetProperty("@type").GetString().Should().Be("Question");
        main[0].GetProperty("name").GetString().Should().Be("Do you sign NDAs?");
        main[0].GetProperty("acceptedAnswer").GetProperty("@type").GetString().Should().Be("Answer");
        main[0].GetProperty("acceptedAnswer").GetProperty("text").GetString()
            .Should().Be("<p>Yes, before details are shared.</p>");
    }

    [Fact]
    public void Build_EmptyList_ReturnsEmptyString()
    {
        FaqJsonLdBuilder.Build([]).Should().Be("");
    }
}
