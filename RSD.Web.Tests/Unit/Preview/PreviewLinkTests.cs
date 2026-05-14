using FluentAssertions;
using Microsoft.Extensions.Options;
using RSD.Web.Services.Preview;

namespace RSD.Web.Tests.Unit.Preview;

public sealed class PreviewLinkTests
{
    private static PreviewLink Build(string key = "unit-test-key", int ttlMinutes = 60)
    {
        var options = Options.Create(new PreviewOptions { SigningKey = key, TtlMinutes = ttlMinutes });
        return new PreviewLink(new HmacPreviewTokenSigner(options), options);
    }

    [Fact]
    public void Build_ProducesUrl_WithTokenForGivenSlug()
    {
        var link = Build();
        var url = link.Build("blog", "hello-world");
        url.Should().StartWith("/preview/blog/hello-world?token=");
    }

    [Fact]
    public void Verify_ReturnsTrue_ForFreshToken()
    {
        var link = Build();
        var url = link.Build("blog", "x");
        var token = url["/preview/blog/x?token=".Length..];
        link.Verify("blog", "x", Uri.UnescapeDataString(token)).Should().BeTrue();
    }

    [Fact]
    public void Verify_ReturnsFalse_OnMismatchedEntityType()
    {
        var link = Build();
        var token = ExtractToken(link.Build("blog", "x"));
        link.Verify("cases", "x", token).Should().BeFalse();
    }

    [Fact]
    public void Verify_ReturnsFalse_OnMismatchedSlug()
    {
        var link = Build();
        var token = ExtractToken(link.Build("blog", "x"));
        link.Verify("blog", "y", token).Should().BeFalse();
    }

    [Fact]
    public void Verify_ReturnsFalse_OnEmptyToken()
    {
        Build().Verify("blog", "x", null).Should().BeFalse();
        Build().Verify("blog", "x", "").Should().BeFalse();
    }

    [Fact]
    public void Verify_ReturnsFalse_AfterKeyRotation()
    {
        var alice = Build("alice");
        var token = ExtractToken(alice.Build("blog", "x"));
        var bob = Build("bob");
        bob.Verify("blog", "x", token).Should().BeFalse();
    }

    [Fact]
    public void Build_WithTtlZeroOrNegative_ProducesAlreadyExpiredToken()
    {
        var link = Build(ttlMinutes: -1);
        var token = ExtractToken(link.Build("blog", "x"));
        link.Verify("blog", "x", token).Should().BeFalse();
    }

    private static string ExtractToken(string url)
    {
        var idx = url.IndexOf("token=", StringComparison.Ordinal);
        return Uri.UnescapeDataString(url[(idx + "token=".Length)..]);
    }
}
