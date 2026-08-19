using FluentAssertions;
using RSD.Web.Services.Seo;

namespace RSD.Web.Tests.Unit.Seo;

public sealed class AbsoluteUrlTests
{
    private const string Root = "https://remsoft.dev";

    [Theory]
    [InlineData("images/logo.svg", "https://remsoft.dev/images/logo.svg")]
    [InlineData("uploads/blog/2026/05/cover.png", "https://remsoft.dev/uploads/blog/2026/05/cover.png")]
    [InlineData("/contact", "https://remsoft.dev/contact")]
    public void Compose_JoinsOriginAndRelativePath(string path, string expected)
    {
        AbsoluteUrl.Compose(Root, path).Should().Be(expected);
    }

    [Fact]
    public void Compose_LeavesAbsoluteUrlsUntouched()
    {
        AbsoluteUrl.Compose(Root, "https://cdn.example.com/x.png").Should().Be("https://cdn.example.com/x.png");
    }

    [Fact]
    public void Compose_EmptyPath_StaysEmpty()
    {
        AbsoluteUrl.Compose(Root, "").Should().Be("");
    }

    [Fact]
    public void Compose_ToleratesTrailingSlashOnOrigin()
    {
        AbsoluteUrl.Compose("https://remsoft.dev/", "images/logo.svg").Should().Be("https://remsoft.dev/images/logo.svg");
    }
}
