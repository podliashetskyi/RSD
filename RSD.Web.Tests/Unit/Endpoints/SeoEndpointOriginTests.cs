using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using RSD.Web.Endpoints;
using RSD.Web.Services.Seo;

namespace RSD.Web.Tests.Unit.Endpoints;

public sealed class SeoEndpointOriginTests
{
    [Fact]
    public async Task Sitemap_UsesConfiguredOrigin_WhenSet()
    {
        var builder = new FakeSitemapBuilder();
        var http = HttpFor("http", "internal-proxy:8080");
        var options = Options.Create(new SeoOptions { BaseUrl = "https://remsoft.dev/" });

        await SitemapEndpoint.HandleAsync(http, builder, options);

        builder.ReceivedBaseUrl.Should().Be("https://remsoft.dev");
    }

    [Fact]
    public async Task Sitemap_FallsBackToRequestHost_WhenUnconfigured()
    {
        var builder = new FakeSitemapBuilder();
        var http = HttpFor("http", "localhost:8082");
        var options = Options.Create(new SeoOptions());

        await SitemapEndpoint.HandleAsync(http, builder, options);

        builder.ReceivedBaseUrl.Should().Be("http://localhost:8082");
    }

    [Fact]
    public void Robots_UsesConfiguredOrigin_WhenSet()
    {
        var provider = new FakeRobotsProvider();
        var http = HttpFor("http", "internal-proxy:8080");
        var options = Options.Create(new SeoOptions { BaseUrl = "https://remsoft.dev" });

        RobotsEndpoint.HandleAsync(http, provider, options);

        provider.ReceivedBaseUrl.Should().Be("https://remsoft.dev");
    }

    private static HttpContext HttpFor(string scheme, string host)
    {
        var http = new DefaultHttpContext();
        http.Request.Scheme = scheme;
        http.Request.Host = new HostString(host);
        return http;
    }

    private sealed class FakeSitemapBuilder : ISitemapBuilder
    {
        public string ReceivedBaseUrl = "";

        public Task<IReadOnlyList<SitemapEntry>> BuildAsync(string baseUrl, CancellationToken ct)
        {
            ReceivedBaseUrl = baseUrl;
            return Task.FromResult<IReadOnlyList<SitemapEntry>>([]);
        }
    }

    private sealed class FakeRobotsProvider : IRobotsTxtProvider
    {
        public string ReceivedBaseUrl = "";

        public string Build(string baseUrl)
        {
            ReceivedBaseUrl = baseUrl;
            return "User-agent: *";
        }
    }
}
