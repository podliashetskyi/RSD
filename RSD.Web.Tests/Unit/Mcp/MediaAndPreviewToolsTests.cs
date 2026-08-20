using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using ModelContextProtocol;
using RSD.Web.Services.Mcp;
using RSD.Web.Services.Mcp.Tools;
using RSD.Web.Services.Preview;

namespace RSD.Web.Tests.Unit.Mcp;

public sealed class MediaAndPreviewToolsTests
{
    [Fact]
    public async Task GetPreviewLink_BuildsAClickableUrl_ForMainTypes()
    {
        var tools = Tools();

        var link = await tools.GetPreviewLinkAsync("blog", "my-draft", CancellationToken.None);

        link.Should().StartWith("http://localhost:8082/preview/blog/my-draft?token=");
    }

    [Fact]
    public async Task GetPreviewLink_RefusesTypesWithoutPreview()
    {
        var act = () => Tools().GetPreviewLinkAsync("faq", "x", CancellationToken.None);

        await act.Should().ThrowAsync<McpException>().WithMessage("*blog*");
    }

    [Fact]
    public async Task UploadImage_MissingFile_GivesAClearError()
    {
        var act = () => Tools().UploadImageAsync("blog", "/nowhere/missing.png", "", CancellationToken.None);

        await act.Should().ThrowAsync<McpException>().WithMessage("*not*found*");
    }

    [Fact]
    public async Task UploadImage_UnsupportedExtension_GivesAClearError()
    {
        var tmp = Path.Combine(Path.GetTempPath(), $"mcp-{Guid.NewGuid():N}.pdf");
        await File.WriteAllTextAsync(tmp, "x");

        var act = () => Tools().UploadImageAsync("blog", tmp, "", CancellationToken.None);

        await act.Should().ThrowAsync<McpException>().WithMessage("*png*");
    }

    private static ContentTools Tools()
    {
        var services = new ServiceCollection();
        services.Configure<PreviewOptions>(o => { o.SigningKey = "test-signing-key"; o.TtlMinutes = 60; });
        services.AddSingleton<IPreviewTokenSigner, HmacPreviewTokenSigner>();
        services.AddSingleton<PreviewLink>();
        services.Configure<McpOptions>(o => { o.PreviewBaseUrl = "http://localhost:8082"; });
        return new ContentTools(services.BuildServiceProvider());
    }
}
