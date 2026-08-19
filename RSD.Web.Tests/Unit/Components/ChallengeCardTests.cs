using Bunit;
using FluentAssertions;
using RSD.Web.Components.Sections.Detail;
using RSD.Web.Data.Entities;

namespace RSD.Web.Tests.Unit.Components;

/// <summary>
/// ChallengeCard is shared by the public Case and Product detail pages. Its meta strip and its
/// hurdle list must render independently: filling only meta used to hide the whole card, silently
/// dropping authored content. These pin each of the four fill states.
/// </summary>
public sealed class ChallengeCardTests
{
    private const string BodyText = "The project faced several critical hurdles:";

    private static List<MetaItem> Meta() => [new("Client", "Acme"), new("Timeframe", "6 months")];
    private static List<ChallengeHurdle> Hurdles() => [new("Legacy data", "Twenty years of records.")];

    [Fact]
    public void MetaOnly_RendersMeta_AndOmitsHurdleListAndBody()
    {
        using var ctx = new BunitContext();

        var cut = ctx.Render<ChallengeCard>(ps => ps
            .Add(p => p.Heading, "The Challenge")
            .Add(p => p.MetaItems, Meta())
            .Add(p => p.Body, BodyText)
            .Add(p => p.Hurdles, []));

        // The regression this guards: meta was hidden entirely when no hurdles existed.
        cut.Markup.Should().Contain("Client").And.Contain("Acme")
                           .And.Contain("Timeframe").And.Contain("6 months");

        cut.FindAll("ul").Should().BeEmpty();      // no hurdles -> no (empty) bullet list
        cut.Markup.Should().NotContain(BodyText);  // Body introduces hurdles, so it must not show alone
    }

    [Fact]
    public void MetaOnly_OmitsDivider_SoTheCardDoesNotEndInADanglingRule()
    {
        using var ctx = new BunitContext();

        var cut = ctx.Render<ChallengeCard>(ps => ps
            .Add(p => p.Heading, "The Challenge")
            .Add(p => p.MetaItems, Meta())
            .Add(p => p.Body, BodyText)
            .Add(p => p.Hurdles, []));

        cut.FindAll("div.border-b").Should().BeEmpty();
    }

    [Fact]
    public void HurdlesOnly_RendersHurdlesAndBody_AndOmitsMetaStrip()
    {
        using var ctx = new BunitContext();

        var cut = ctx.Render<ChallengeCard>(ps => ps
            .Add(p => p.Heading, "The Challenge")
            .Add(p => p.MetaItems, [])
            .Add(p => p.Body, BodyText)
            .Add(p => p.Hurdles, Hurdles()));

        cut.Markup.Should().Contain(BodyText)
                           .And.Contain("Legacy data")
                           .And.Contain("Twenty years of records.");

        cut.Markup.Should().NotContain("Client");        // no meta -> no meta strip
        cut.FindAll("div.border-b").Should().BeEmpty();  // nothing above the hurdles to divide
    }

    [Fact]
    public void MetaAndHurdles_RenderBoth_WithDividerBetweenThem()
    {
        using var ctx = new BunitContext();

        var cut = ctx.Render<ChallengeCard>(ps => ps
            .Add(p => p.Heading, "The Challenge")
            .Add(p => p.MetaItems, Meta())
            .Add(p => p.Body, BodyText)
            .Add(p => p.Hurdles, Hurdles()));

        cut.Markup.Should().Contain("Client").And.Contain("Acme")   // meta strip
                           .And.Contain(BodyText)                   // body
                           .And.Contain("Legacy data");             // hurdle list

        cut.FindAll("ul").Should().ContainSingle();
        cut.FindAll("div.border-b").Should().ContainSingle();       // divider iff hurdles follow meta
    }

    [Fact]
    public void Neither_RendersHeadingOnly_WithoutEmptyScaffolding()
    {
        using var ctx = new BunitContext();

        var cut = ctx.Render<ChallengeCard>(ps => ps
            .Add(p => p.Heading, "The Challenge")
            .Add(p => p.MetaItems, [])
            .Add(p => p.Body, BodyText)
            .Add(p => p.Hurdles, []));

        // Pages guard against this case, but the component must still degrade cleanly.
        cut.Markup.Should().Contain("The Challenge");
        cut.FindAll("ul").Should().BeEmpty();
        cut.Markup.Should().NotContain(BodyText);
    }
}
