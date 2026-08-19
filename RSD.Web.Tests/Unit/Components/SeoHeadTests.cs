using Bunit;
using FluentAssertions;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using RSD.Web.Components.Shared;
using RSD.Web.Services.Seo;

namespace RSD.Web.Tests.Unit.Components;

/// <summary>
/// SeoHead is the single component that renders canonical, description, Open Graph, Twitter,
/// and robots tags into the head. These pin the tag set and the URL composition rules.
/// </summary>
public sealed class SeoHeadTests
{
    private const string Origin = "https://remsoft.dev";

    [Fact]
    public void EmitsCanonical_Description_OpenGraph_AndTwitterTags()
    {
        using var ctx = ContextAt("/blog/my-post");

        var markup = RenderSeoHead(ctx, title: "My Post", description: "A useful post.",
            ogImagePath: "uploads/blog/cover.png", ogImageAlt: "Cover", type: "article");

        markup.Should().Contain($"<link rel=\"canonical\" href=\"{Origin}/blog/my-post\"");
        markup.Should().Contain("name=\"description\" content=\"A useful post.\"");
        markup.Should().Contain("property=\"og:title\" content=\"My Post\"");
        markup.Should().Contain("property=\"og:description\" content=\"A useful post.\"");
        markup.Should().Contain($"property=\"og:url\" content=\"{Origin}/blog/my-post\"");
        markup.Should().Contain($"property=\"og:image\" content=\"{Origin}/uploads/blog/cover.png\"");
        markup.Should().Contain("property=\"og:image:alt\" content=\"Cover\"");
        markup.Should().Contain("property=\"og:type\" content=\"article\"");
        markup.Should().Contain("name=\"twitter:card\" content=\"summary_large_image\"");
        markup.Should().Contain($"name=\"twitter:image\" content=\"{Origin}/uploads/blog/cover.png\"");
    }

    [Fact]
    public void NoRobotsMeta_ByDefault()
    {
        using var ctx = ContextAt("/about");

        RenderSeoHead(ctx, title: "About", description: "d").Should().NotContain("name=\"robots\"");
    }

    [Fact]
    public void RobotsMeta_EmittedWhenSet()
    {
        using var ctx = ContextAt("/about");

        RenderSeoHead(ctx, title: "About", description: "d", robots: "noindex")
            .Should().Contain("name=\"robots\" content=\"noindex\"");
    }

    [Fact]
    public void CanonicalPathOverride_Wins_AndStripsNothingElse()
    {
        using var ctx = ContextAt("/cases?tech=dotnet&page=2");

        var markup = RenderSeoHead(ctx, title: "Cases", description: "d", canonicalPath: "/cases");

        markup.Should().Contain($"<link rel=\"canonical\" href=\"{Origin}/cases\"");
    }

    [Fact]
    public void CanonicalUsesRequestPath_WithoutQueryString_ByDefault()
    {
        using var ctx = ContextAt("/blog?category=ai");

        RenderSeoHead(ctx, title: "Blog", description: "d")
            .Should().Contain($"<link rel=\"canonical\" href=\"{Origin}/blog\"");
    }

    [Fact]
    public void MissingOgImage_FallsBackToDefaultOgAsset()
    {
        using var ctx = ContextAt("/about");

        RenderSeoHead(ctx, title: "About", description: "d")
            .Should().Contain($"property=\"og:image\" content=\"{Origin}/images/og-default.png\"");
    }

    [Fact]
    public void EmptyDescription_EmitsNoDescriptionTags()
    {
        using var ctx = ContextAt("/about");

        var markup = RenderSeoHead(ctx, title: "About", description: "");

        markup.Should().NotContain("name=\"description\"");
        markup.Should().NotContain("property=\"og:description\"");
    }

    private static BunitContext ContextAt(string pathAndQuery)
    {
        var ctx = new BunitContext();
        ctx.JSInterop.Mode = JSRuntimeMode.Loose;
        var http = new DefaultHttpContext();
        var parts = pathAndQuery.Split('?', 2);
        http.Request.Path = parts[0];
        if (parts.Length == 2) http.Request.QueryString = new QueryString($"?{parts[1]}");
        ctx.Services.AddSingleton<IHttpContextAccessor>(new HttpContextAccessor { HttpContext = http });
        ctx.Services.AddSingleton(Options.Create(new SeoOptions { BaseUrl = Origin }));
        return ctx;
    }

    private static string RenderSeoHead(BunitContext ctx, string title, string description,
        string ogImagePath = "", string ogImageAlt = "", string type = "website",
        string robots = "", string canonicalPath = "")
    {
        var cut = ctx.Render(builder =>
        {
            builder.OpenComponent<HeadOutlet>(0);
            builder.CloseComponent();
            builder.OpenComponent<SeoHead>(1);
            builder.AddComponentParameter(2, nameof(SeoHead.Title), title);
            builder.AddComponentParameter(3, nameof(SeoHead.Description), description);
            builder.AddComponentParameter(4, nameof(SeoHead.OgImagePath), ogImagePath);
            builder.AddComponentParameter(5, nameof(SeoHead.OgImageAlt), ogImageAlt);
            builder.AddComponentParameter(6, nameof(SeoHead.Type), type);
            builder.AddComponentParameter(7, nameof(SeoHead.Robots), robots);
            builder.AddComponentParameter(8, nameof(SeoHead.CanonicalPath), canonicalPath);
            builder.CloseComponent();
        });
        return cut.Markup;
    }
}
