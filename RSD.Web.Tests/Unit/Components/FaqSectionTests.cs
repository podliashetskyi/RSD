using System.Text.Json;
using Bunit;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using RSD.Web.Components.Sections.Shared;
using RSD.Web.Data.Entities;
using RSD.Web.Services.Content;

namespace RSD.Web.Tests.Unit.Components;

/// <summary>
/// Public FAQ section: question-shaped h3 headings, answer HTML rendered, sitewide items
/// only (page-scoped ones wait for their detail pages), and a parity FAQPage schema block.
/// </summary>
public sealed class FaqSectionTests
{
    private static FaqItem Faq(string q, int order, string ownerSlug = "", bool showOnHome = false) => new()
    {
        Slug = q, Question = q, AnswerHtml = $"<p>Answer to {q}</p>",
        DisplayOrder = order, OwnerSlug = ownerSlug, ShowOnHome = showOnHome, Status = ContentStatus.Published,
    };

    [Fact]
    public void RendersQuestions_AsCollapsedAccordion_PinnedFirst_WithFaqPageSchema()
    {
        using var ctx = new BunitContext();
        ctx.Services.AddSingleton<IFaqItemService>(new FakeFaqItemService(
        [
            Faq("Second?", 2),
            Faq("First?", 1),
            Faq("Pinned?", 9, showOnHome: true),
            Faq("Scoped?", 0, ownerSlug: "some-page"),
        ]));

        var cut = ctx.Render<FaqSection>();

        // Accordion: each item is a <details> with the question in <summary>, collapsed by default.
        var summaries = cut.FindAll("details summary").Select(s => s.TextContent.Trim()).ToList();
        summaries.Should().ContainInOrder("Pinned?", "First?", "Second?");
        summaries.Should().NotContain("Scoped?");
        cut.FindAll("details[open]").Should().BeEmpty();
        cut.Markup.Should().Contain("<p>Answer to First?</p>");

        var json = ExtractScriptBody(cut.Markup);
        using var doc = JsonDocument.Parse(json);
        doc.RootElement.GetProperty("@type").GetString().Should().Be("FAQPage");
        doc.RootElement.GetProperty("mainEntity").GetArrayLength().Should().Be(3);
    }

    [Fact]
    public void PinnedOnlyMode_ShowsAtMostFourPinnedItems()
    {
        using var ctx = new BunitContext();
        ctx.Services.AddSingleton<IFaqItemService>(new FakeFaqItemService(
        [
            Faq("P1?", 1, showOnHome: true),
            Faq("P2?", 2, showOnHome: true),
            Faq("P3?", 3, showOnHome: true),
            Faq("P4?", 4, showOnHome: true),
            Faq("P5?", 5, showOnHome: true),
            Faq("Unpinned?", 0),
        ]));

        var cut = ctx.Render<FaqSection>(p => p.Add(x => x.PinnedOnly, true));

        var summaries = cut.FindAll("details summary").Select(s => s.TextContent.Trim()).ToList();
        summaries.Should().Equal("P1?", "P2?", "P3?", "P4?");
    }

    [Fact]
    public void PinnedOnlyMode_WithNothingPinned_RendersNothing()
    {
        using var ctx = new BunitContext();
        ctx.Services.AddSingleton<IFaqItemService>(new FakeFaqItemService([Faq("Unpinned?", 1)]));

        var cut = ctx.Render<FaqSection>(p => p.Add(x => x.PinnedOnly, true));

        cut.Markup.Trim().Should().BeEmpty();
    }

    [Fact]
    public void NoSitewideItems_RendersNothing()
    {
        using var ctx = new BunitContext();
        ctx.Services.AddSingleton<IFaqItemService>(new FakeFaqItemService([Faq("Scoped?", 1, "some-page")]));

        var cut = ctx.Render<FaqSection>();

        cut.Markup.Trim().Should().BeEmpty();
    }

    private static string ExtractScriptBody(string markup)
    {
        var start = markup.IndexOf('>', markup.IndexOf("<script", StringComparison.Ordinal)) + 1;
        var end = markup.IndexOf("</script>", StringComparison.Ordinal);
        return markup[start..end];
    }
}
