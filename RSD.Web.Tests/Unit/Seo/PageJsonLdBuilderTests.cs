using System.Text.Json;
using FluentAssertions;
using RSD.Web.Data.Entities;
using RSD.Web.Services.Seo;

namespace RSD.Web.Tests.Unit.Seo;

/// <summary>
/// Per-page JSON-LD: BlogPosting (with a real Person author), Service, Product.
/// Every node must reference the sitewide #organization anchor and use absolute URLs.
/// </summary>
public sealed class PageJsonLdBuilderTests
{
    private const string Origin = "https://remsoft.dev";

    private static BlogPost Post() => new()
    {
        Slug = "my-post",
        Title = "My Post",
        Summary = "Short summary.",
        Category = "AI",
        Tags = ["dotnet", "blazor"],
        CoverImagePath = "images/blog/cover.png",
        PublishedAt = new DateTime(2026, 5, 1, 10, 0, 0, DateTimeKind.Utc),
        UpdatedAt = new DateTime(2026, 6, 2, 9, 0, 0, DateTimeKind.Utc),
    };

    [Fact]
    public void BlogPosting_CarriesDates_Taxonomy_Image_AndOrganizationPublisher()
    {
        var json = PageJsonLdBuilder.BlogPosting(Origin, Post(), author: null);
        using var doc = JsonDocument.Parse(json);
        var node = doc.RootElement;

        node.GetProperty("@type").GetString().Should().Be("BlogPosting");
        node.GetProperty("headline").GetString().Should().Be("My Post");
        node.GetProperty("description").GetString().Should().Be("Short summary.");
        node.GetProperty("image").GetString().Should().Be($"{Origin}/images/blog/cover.png");
        node.GetProperty("datePublished").GetString().Should().Be("2026-05-01");
        node.GetProperty("dateModified").GetString().Should().Be("2026-06-02");
        node.GetProperty("articleSection").GetString().Should().Be("AI");
        node.GetProperty("keywords")[1].GetString().Should().Be("blazor");
        node.GetProperty("mainEntityOfPage").GetString().Should().Be($"{Origin}/blog/my-post");
        node.GetProperty("publisher").GetProperty("@id").GetString().Should().Be($"{Origin}#organization");
        // No author row -> the organization itself is the author.
        node.GetProperty("author").GetProperty("@id").GetString().Should().Be($"{Origin}#organization");
    }

    [Fact]
    public void BlogPosting_WithTeamMemberAuthor_EmitsPersonWithSameAs()
    {
        var author = new TeamMember
        {
            Slug = "jane",
            Name = "Jane Dev",
            Role = "Principal Engineer",
            LinkedInUrl = "https://www.linkedin.com/in/janedev",
            GitHubUrl = "https://github.com/janedev",
        };

        var json = PageJsonLdBuilder.BlogPosting(Origin, Post(), author);
        using var doc = JsonDocument.Parse(json);
        var person = doc.RootElement.GetProperty("author");

        person.GetProperty("@type").GetString().Should().Be("Person");
        person.GetProperty("name").GetString().Should().Be("Jane Dev");
        person.GetProperty("jobTitle").GetString().Should().Be("Principal Engineer");
        person.GetProperty("sameAs").GetArrayLength().Should().Be(2);
    }

    [Fact]
    public void BlogPosting_FallsBackToCreatedAt_WhenNeverPublished()
    {
        var post = Post() with { PublishedAt = null };

        var json = PageJsonLdBuilder.BlogPosting(Origin, post, author: null);
        using var doc = JsonDocument.Parse(json);

        doc.RootElement.GetProperty("datePublished").GetString()
            .Should().Be(post.CreatedAt.ToString("yyyy-MM-dd"));
    }

    [Fact]
    public void BlogPosting_OmitsEmptyDescription()
    {
        var post = Post() with { Summary = "" };

        var json = PageJsonLdBuilder.BlogPosting(Origin, post, author: null);
        using var doc = JsonDocument.Parse(json);

        doc.RootElement.TryGetProperty("description", out _).Should().BeFalse();
    }

    [Fact]
    public void Service_EmitsProviderReference()
    {
        var svc = new Service { Slug = "cloud", Title = "Cloud Solutions", Summary = "We migrate you." };

        var json = PageJsonLdBuilder.ServiceNode(Origin, svc);
        using var doc = JsonDocument.Parse(json);
        var node = doc.RootElement;

        node.GetProperty("@type").GetString().Should().Be("Service");
        node.GetProperty("name").GetString().Should().Be("Cloud Solutions");
        node.GetProperty("description").GetString().Should().Be("We migrate you.");
        node.GetProperty("url").GetString().Should().Be($"{Origin}/services/cloud");
        node.GetProperty("provider").GetProperty("@id").GetString().Should().Be($"{Origin}#organization");
    }

    [Fact]
    public void Product_EmitsNameDescriptionAndImage_WithoutOffers()
    {
        var product = new Product { Slug = "nexa", Name = "NexaCRM", Summary = "A CRM.", CoverImagePath = "images/products/nexa.png" };

        var json = PageJsonLdBuilder.ProductNode(Origin, product);
        using var doc = JsonDocument.Parse(json);
        var node = doc.RootElement;

        node.GetProperty("@type").GetString().Should().Be("Product");
        node.GetProperty("name").GetString().Should().Be("NexaCRM");
        node.GetProperty("image").GetString().Should().Be($"{Origin}/images/products/nexa.png");
        node.GetProperty("url").GetString().Should().Be($"{Origin}/products/nexa");
        // Free-text price is not machine-readable; structured offers arrive in Tier 3.
        node.TryGetProperty("offers", out _).Should().BeFalse();
    }
}
