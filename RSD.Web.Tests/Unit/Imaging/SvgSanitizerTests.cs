using FluentAssertions;
using RSD.Web.Services.Imaging;

namespace RSD.Web.Tests.Unit.Imaging;

public sealed class SvgSanitizerTests
{
    private static readonly SvgSanitizer Sut = new();

    [Fact]
    public void Sanitize_StripsScriptTags()
    {
        const string malicious = """
            <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 10 10">
                <script>alert(1)</script>
                <rect width="10" height="10" fill="red"/>
            </svg>
            """;

        var clean = Sut.Sanitize(malicious);
        clean.Should().NotContain("<script");
        clean.Should().Contain("<rect");
    }

    [Fact]
    public void Sanitize_StripsEventHandlerAttributes()
    {
        const string malicious = """<svg xmlns="http://www.w3.org/2000/svg"><circle onload="alert(1)" cx="5" cy="5" r="3"/></svg>""";

        var clean = Sut.Sanitize(malicious);
        clean.Should().NotContain("onload");
        clean.Should().NotContain("alert");
    }

    [Fact]
    public void Sanitize_KeepsBenignPathContent()
    {
        const string svg = """<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24"><path d="M12 0L24 12L12 24L0 12Z" fill="#333"/></svg>""";

        var clean = Sut.Sanitize(svg);
        clean.Should().Contain("<path");
        clean.Should().Contain("d=");
    }
}
