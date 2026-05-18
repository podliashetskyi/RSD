using FluentAssertions;
using RSD.Web.Services.Content;

namespace RSD.Web.Tests.Unit.Content;

public sealed class LinkHrefValidatorTests
{
    [Theory]
    [InlineData("")]
    [InlineData("https://remsoft.dev")]
    public void SocialHref_AllowsEmptyOrHttps(string href)
    {
        LinkHrefValidator.IsValidSocialHref(href).Should().BeTrue();
    }

    [Theory]
    [InlineData("#")]
    [InlineData("google.com")]
    [InlineData("http://remsoft.dev")]
    [InlineData("mailto:contactus@remsoft.dev")]
    [InlineData(" https://remsoft.dev")]
    public void SocialHref_RejectsFakeBareOrUnsupportedUrls(string href)
    {
        LinkHrefValidator.IsValidSocialHref(href).Should().BeFalse();
    }

    [Theory]
    [InlineData("")]
    [InlineData("https://wa.me/123456789")]
    [InlineData("tg://resolve?domain=remsoftdev")]
    [InlineData("viber://chat?number=123456789")]
    [InlineData("whatsapp://send?phone=123456789")]
    public void MessengerHref_AllowsConfiguredSchemes(string href)
    {
        LinkHrefValidator.IsValidMessengerHref(href).Should().BeTrue();
    }

    [Theory]
    [InlineData("#")]
    [InlineData("google.com")]
    [InlineData("http://wa.me/123456789")]
    [InlineData("mailto:contactus@remsoft.dev")]
    [InlineData(" https://wa.me/123456789")]
    public void MessengerHref_RejectsFakeBareOrUnsupportedUrls(string href)
    {
        LinkHrefValidator.IsValidMessengerHref(href).Should().BeFalse();
    }
}
