using FluentAssertions;
using RSD.Web.Services.Common;

namespace RSD.Web.Tests.Unit.Content;

public sealed class ContentHtmlSanitizerTests
{
    private static readonly IContentHtmlSanitizer Sanitizer = new ContentHtmlSanitizer();

    [Fact]
    public void StripsScriptTag()
    {
        var html = "<p>hi</p><script>alert('x')</script>";
        Sanitizer.Sanitize(html).Should().Be("<p>hi</p>");
    }

    [Fact]
    public void StripsInlineEventHandlers()
    {
        var html = "<a href=\"https://example.com\" onclick=\"alert('x')\">go</a>";
        Sanitizer.Sanitize(html).Should().NotContain("onclick");
    }

    [Fact]
    public void StripsDisallowedScheme()
    {
        Sanitizer.Sanitize("<a href=\"javascript:alert(1)\">x</a>").Should().NotContain("javascript:");
    }

    [Fact]
    public void PreservesBoldItalicUnderlineAndLink()
    {
        var html = "<p><strong>bold</strong> <em>italic</em> <u>under</u> <a href=\"https://r.com\">link</a></p>";
        var result = Sanitizer.Sanitize(html);
        result.Should().Contain("<strong>bold</strong>");
        result.Should().Contain("<em>italic</em>");
        result.Should().Contain("<u>under</u>");
        result.Should().Contain("href=\"https://r.com\"");
    }

    [Fact]
    public void PreservesHeadingsAndLists()
    {
        var html = "<h2>title</h2><ul><li>a</li></ul><ol><li>b</li></ol>";
        Sanitizer.Sanitize(html).Should().Be(html);
    }

    [Fact]
    public void EmptyOrWhitespaceReturnsEmpty()
    {
        Sanitizer.Sanitize("").Should().Be("");
        Sanitizer.Sanitize("   ").Should().Be("");
    }
}
