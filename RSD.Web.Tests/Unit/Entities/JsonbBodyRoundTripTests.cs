using System.Text.Json;
using FluentAssertions;
using RSD.Web.Data.Configurations;
using RSD.Web.Data.Entities;
using RSD.Web.Data.Entities.ArticleBlocks;

namespace RSD.Web.Tests.Unit.Entities;

public sealed class JsonbBodyRoundTripTests
{
    private static readonly JsonSerializerOptions Options = JsonbValueConverter.SerializerOptions;

    [Fact]
    public void CaseDetailFields_RoundTrips_AllValuesPreserved()
    {
        var original = new CaseDetailFields
        {
            Badges = [new("Healthcare", "bg-cyan-100", "text-cyan-700"), new("23 months", "bg-cyan-100", "text-cyan-700")],
            MetaTags = ["Flutter", "AWS", "IoT", "HIPAA"],
            Meta = [new("Industry", "Healthcare"), new("Timeframe", "2024–2026")],
            Hurdles = [new("Real-Time Reliability", "Sub-second updates are non-negotiable.")],
            Results = ["50,000+ active patients", "99.9% uptime achieved"],
            TechPills = ["Flutter", "Dart", "AWS Lambda"],
            Metrics = [new("Reduce churn 40%", "AI-powered retention models.")],
            Testimonial = new("Best telehealth product we've used.", "Dr. Jane", "CMO", "/img/jane.png"),
            Conclusion = new("Mission-critical telemedicine.", "Built for scale and trust.")
        };

        var json = JsonSerializer.Serialize(original, Options);
        var round = JsonSerializer.Deserialize<CaseDetailFields>(json, Options)!;

        round.Should().BeEquivalentTo(original);
    }

    [Fact]
    public void ProductDetailFields_RoundTrips_AllValuesPreserved()
    {
        var original = new ProductDetailFields
        {
            Badges = [new("Available", "bg-success-100", "text-success-600")],
            Features = ["AI Sales Forecasting", "Custom Workflows"],
            ChallengeMeta = [new("Product", "CRM"), new("Timeframe", "2026")],
            Hurdles = [new("Adoption", "Move users off legacy.")],
            Results = ["360° view", "+40% sales"],
            Metrics = [new("Save 10h Weekly", "Automation handles routine.")],
            TechPills = ["Flutter", "FHIR API"]
        };

        var json = JsonSerializer.Serialize(original, Options);
        var round = JsonSerializer.Deserialize<ProductDetailFields>(json, Options)!;

        round.Should().BeEquivalentTo(original);
    }

    [Fact]
    public void ArticleBody_PolymorphicBlocks_RoundTrip()
    {
        var original = new ArticleBody
        {
            Intro = "Welcome to the article.",
            Blocks =
            [
                new SubsectionBlock
                {
                    Id = "sub-1",
                    Heading = "Key Features",
                    Subheading = "subhead",
                    Body = "<p>body</p>",
                    Items = [new("High Availability:", "Zero downtime."), new("Cost Efficiency:", "Smart allocation.")]
                },
                new StatsRowBlock
                {
                    Id = "stats-1",
                    Heading = "Numbers",
                    Items = [new("99.9%", "Uptime"), new("40%", "Cost cut")]
                },
                new GalleryBlock
                {
                    Id = "gallery-1",
                    Heading = "Visuals",
                    Description = "shots",
                    Images = [new("/a.png", "A"), new("/b.png", "B")],
                    Tags = ["screens", "diagrams"]
                },
                new BulletListBlock { Id = "bullets-1", Heading = "Tools", Items = ["AWS", "GCP"] },
                new QuoteBlock { Id = "quote-1", Quote = "Build fast.", Attribution = "Anon" },
                new ImageBlock { Id = "image-1", ImagePath = "/img.png", Caption = "cap", Alt = "alt" },
                new RichTextBlock { Id = "rt-1", Html = "<p>Hello</p>" },
            ]
        };

        var json = JsonSerializer.Serialize(original, Options);
        var round = JsonSerializer.Deserialize<ArticleBody>(json, Options)!;

        round.Intro.Should().Be(original.Intro);
        round.Blocks.Should().HaveCount(original.Blocks.Count);
        for (var i = 0; i < original.Blocks.Count; i++)
        {
            round.Blocks[i].Should().BeOfType(original.Blocks[i].GetType());
            round.Blocks[i].Should().BeEquivalentTo(original.Blocks[i]);
        }
    }

    [Fact]
    public void ArticleBody_DiscriminatorField_IsPresent()
    {
        var body = new ArticleBody { Blocks = [new QuoteBlock { Id = "q", Quote = "x", Attribution = "y" }] };
        var json = JsonSerializer.Serialize(body, Options);
        json.Should().Contain("\"$type\":\"quote\"");
    }
}
