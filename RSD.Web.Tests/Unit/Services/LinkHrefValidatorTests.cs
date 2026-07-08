using FluentAssertions;
using RSD.Web.Services.Content;

namespace RSD.Web.Tests.Unit.Services;

public sealed class LinkHrefValidatorTests
{
    [Theory]
    [InlineData("")]                               // blank allowed (renders as plain text)
    [InlineData("https://example.com")]
    [InlineData("mailto:contactus@remsoft.dev")]
    [InlineData("tel:+14155551234")]
    public void IsValidContactHref_AcceptsBlankHttpsMailtoTel(string href) =>
        LinkHrefValidator.IsValidContactHref(href).Should().BeTrue();

    [Theory]
    [InlineData("javascript:alert(1)")]
    [InlineData("ftp://example.com")]
    [InlineData("http://example.com")]             // plain http not allowed, matching social/messenger policy
    [InlineData(" https://example.com")]           // leading whitespace rejected
    public void IsValidContactHref_RejectsOtherSchemes(string href) =>
        LinkHrefValidator.IsValidContactHref(href).Should().BeFalse();
}
