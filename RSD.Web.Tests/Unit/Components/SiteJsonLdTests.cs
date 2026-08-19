using System.Text.Json;
using Bunit;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using RSD.Web.Components.Shared;
using RSD.Web.Data.Entities;
using RSD.Web.Services.Content;
using RSD.Web.Services.Seo;

namespace RSD.Web.Tests.Unit.Components;

/// <summary>
/// SiteJsonLd must emit a literal ld+json script element into the static page body —
/// Blazor treats script elements specially, so this guards the emission mechanism itself.
/// </summary>
public sealed class SiteJsonLdTests
{
    [Fact]
    public void RendersLdJsonScript_WithParsableGraph_AndFooterSameAs()
    {
        using var ctx = new BunitContext();
        var http = new DefaultHttpContext();
        http.Request.Path = "/";
        ctx.Services.AddSingleton<IHttpContextAccessor>(new HttpContextAccessor { HttpContext = http });
        ctx.Services.AddSingleton(Options.Create(new SeoOptions { BaseUrl = "https://remsoft.dev" }));
        ctx.Services.AddSingleton<ISocialLinkService>(new FakeSocialLinkService(
        [
            new SocialLink { Slug = "li", Label = "LinkedIn", Href = "https://www.linkedin.com/company/rsd", Scope = SocialLinkScope.Footer, Status = ContentStatus.Published },
        ]));

        var cut = ctx.Render<SiteJsonLd>();

        cut.Markup.Should().Contain("application/ld+json");
        var json = ExtractScriptBody(cut.Markup);
        using var doc = JsonDocument.Parse(json);
        doc.RootElement.GetProperty("@graph")[0].GetProperty("sameAs")[0].GetString()
            .Should().Be("https://www.linkedin.com/company/rsd");
    }

    private static string ExtractScriptBody(string markup)
    {
        var start = markup.IndexOf('>', markup.IndexOf("<script", StringComparison.Ordinal)) + 1;
        var end = markup.IndexOf("</script>", StringComparison.Ordinal);
        return markup[start..end];
    }
}
