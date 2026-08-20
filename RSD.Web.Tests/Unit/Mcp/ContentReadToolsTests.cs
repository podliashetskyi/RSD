using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using RSD.Web.Data.Entities;
using RSD.Web.Services.Content;
using RSD.Web.Services.Mcp.Tools;
using RSD.Web.Tests.Unit.Components;

namespace RSD.Web.Tests.Unit.Mcp;

/// <summary>
/// The MCP read surface: every content type listable and gettable (drafts included),
/// taxonomy readable, unknown keys rejected with a helpful error.
/// </summary>
public sealed class ContentReadToolsTests
{
    private static readonly string[] AllTypeKeys =
    [
        "blog", "cases", "products", "services",
        "testimonials", "team", "partners", "values", "stats", "tech",
        "contact-points", "messenger-links", "social-links", "faq",
        "terms-of-service", "privacy-policy",
    ];

    [Fact]
    public void Registry_KnowsEveryContentType()
    {
        ContentTypeRegistry.Keys.Should().BeEquivalentTo(AllTypeKeys);
    }

    [Fact]
    public async Task ListContent_MainType_UsesNameAsTitle()
    {
        var sp = Provider(s => s.AddSingleton<ICaseService>(new FakeCaseService(
            [new Case { Slug = "hc", Name = "Healthcare Plus", Status = ContentStatus.Draft }])));

        var rows = await new ContentTools(sp).ListContentAsync("cases", CancellationToken.None);

        rows.Should().ContainSingle(r => r.Title == "Healthcare Plus" && r.Status == "Draft");
    }

    [Fact]
    public async Task ListContent_SimpleType_UsesNaturalTitle()
    {
        var sp = Provider(s => s.AddSingleton<IFaqItemService>(new FakeFaqItemService(
            [new FaqItem { Slug = "q1", Question = "Do you sign NDAs?" }])));

        var rows = await new ContentTools(sp).ListContentAsync("faq", CancellationToken.None);

        rows.Should().ContainSingle(r => r.Title == "Do you sign NDAs?");
    }

    [Fact]
    public async Task GetContent_BySlug_ReturnsDrafts()
    {
        var draft = new BlogPost { Slug = "draft-post", Title = "Draft", Status = ContentStatus.Draft };
        var sp = Provider(s => s.AddSingleton<IBlogService>(new FakeBlogService([draft])));

        var result = await new ContentTools(sp).GetContentAsync("blog", "draft-post", CancellationToken.None);

        result.Should().BeOfType<BlogPost>().Which.Title.Should().Be("Draft");
    }

    [Fact]
    public async Task GetContent_ById_Works_ForSimpleTypes()
    {
        var item = new Testimonial { Slug = "t", Title = "Great", Quote = "Q", AuthorName = "A" };
        var sp = Provider(s => s.AddSingleton<ITestimonialService>(new FakeTestimonialService([item])));

        var result = await new ContentTools(sp).GetContentAsync("testimonials", item.Id.ToString(), CancellationToken.None);

        result.Should().BeOfType<Testimonial>().Which.Title.Should().Be("Great");
    }

    [Fact]
    public async Task GetContent_UnknownSlug_ThrowsHelpfulError()
    {
        var sp = Provider(s => s.AddSingleton<IBlogService>(new FakeBlogService([])));

        var act = () => new ContentTools(sp).GetContentAsync("blog", "missing", CancellationToken.None);

        await act.Should().ThrowAsync<ModelContextProtocol.McpException>().WithMessage("*missing*");
    }

    [Fact]
    public async Task ListContent_UnknownType_ThrowsWithSupportedKeys()
    {
        var act = () => new ContentTools(Provider(_ => { })).ListContentAsync("wat", CancellationToken.None);

        await act.Should().ThrowAsync<ModelContextProtocol.McpException>().WithMessage("*blog*");
    }

    [Fact]
    public async Task ListFilters_ReturnsPublishedLabels_ForTheRequestedTaxonomy()
    {
        var sp = Provider(s => s.AddSingleton<IFilterService>(new FakeFilterService(
        [
            new Filter { Slug = "ai", Label = "AI", Type = FilterType.BlogCategory },
            new Filter { Slug = "react", Label = "React", Type = FilterType.CaseTechTag },
        ])));

        var labels = await new ContentTools(sp).ListFiltersAsync("BlogCategory", CancellationToken.None);

        labels.Should().Equal("AI");
    }

    private static IServiceProvider Provider(Action<ServiceCollection> configure)
    {
        var services = new ServiceCollection();
        configure(services);
        return services.BuildServiceProvider();
    }
}

internal sealed class FakeTestimonialService(IReadOnlyList<Testimonial> items)
    : FakeContentService<Testimonial>(items), ITestimonialService;
