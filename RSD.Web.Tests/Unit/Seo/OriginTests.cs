using FluentAssertions;
using Microsoft.AspNetCore.Http;
using RSD.Web.Services.Seo;

namespace RSD.Web.Tests.Unit.Seo;

public sealed class OriginTests
{
    [Theory]
    [InlineData("https://remsoft.dev", "https://remsoft.dev")]
    [InlineData("https://remsoft.dev/", "https://remsoft.dev")]
    [InlineData("https://remsoft.dev///", "https://remsoft.dev")]
    public void Origin_TrimsTrailingSlashes(string input, string expected)
    {
        new Origin(input).Value.Should().Be(expected);
    }

    [Theory]
    [InlineData("", true)]
    [InlineData("/", true)]
    [InlineData("https://remsoft.dev", false)]
    public void Origin_IsEmpty_ReflectsValue(string input, bool expected)
    {
        new Origin(input).IsEmpty.Should().Be(expected);
    }

    [Fact]
    public void Resolve_PrefersConfiguredBaseUrl_OverRequestHost()
    {
        var options = new SeoOptions { BaseUrl = "https://remsoft.dev/" };
        var request = RequestWith("http", "localhost:8082");

        RequestOrigin.Resolve(options, request).Should().Be("https://remsoft.dev");
    }

    [Fact]
    public void Resolve_FallsBackToRequestSchemeAndHost_WhenUnconfigured()
    {
        var options = new SeoOptions();
        var request = RequestWith("http", "localhost:8082");

        RequestOrigin.Resolve(options, request).Should().Be("http://localhost:8082");
    }

    private static HttpRequest RequestWith(string scheme, string host)
    {
        var http = new DefaultHttpContext();
        http.Request.Scheme = scheme;
        http.Request.Host = new HostString(host);
        return http.Request;
    }
}
