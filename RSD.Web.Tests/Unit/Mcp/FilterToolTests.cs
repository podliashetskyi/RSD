using System.Text.Json;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using ModelContextProtocol;
using RSD.Web.Data.Entities;
using RSD.Web.Services.Content;
using RSD.Web.Services.Mcp.Tools;
using RSD.Web.Tests.Unit.Components;

namespace RSD.Web.Tests.Unit.Mcp;

/// <summary>
/// Taxonomy via MCP: filters can be CREATED (draft-first, published via publish_content)
/// but never renamed/updated — renames orphan tagged content and stay a deliberate
/// admin-UI action.
/// </summary>
public sealed class FilterToolTests
{
    [Fact]
    public async Task CreateFilter_ForcesDraft_AndParsesTheTypeEnum()
    {
        var svc = new FakeFilterService();
        await Tools(svc).CreateContentAsync("filters",
            Json("""{"slug":"","type":"CaseIndustry","label":"Healthcare","displayOrder":1,"status":"Published"}"""),
            CancellationToken.None);

        var filter = svc.LastCreate.Should().BeOfType<Filter>().Subject;
        filter.Status.Should().Be(ContentStatus.Draft);
        filter.Type.Should().Be(FilterType.CaseIndustry);
        filter.Label.Should().Be("Healthcare");
    }

    [Fact]
    public async Task UpdateFilter_IsAlwaysRefused_RenamesAreAdminOnly()
    {
        var existing = new Filter { Slug = "ai", Type = FilterType.BlogCategory, Label = "AI", Status = ContentStatus.Draft };
        var svc = new FakeFilterService([existing]);

        var act = () => Tools(svc).UpdateContentAsync("filters", existing.Id.ToString(),
            Json($$"""{"id":"{{existing.Id}}","slug":"ai","type":"BlogCategory","label":"Artificial Intelligence"}"""),
            allowLiveEdit: true, CancellationToken.None);

        await act.Should().ThrowAsync<McpException>().WithMessage("*admin*");
        svc.LastUpdate.Should().BeNull();
    }

    [Fact]
    public async Task PublishFilter_SetsStatusPublished_SoItAppearsInPickers()
    {
        var draft = new Filter { Slug = "hc", Type = FilterType.CaseIndustry, Label = "Healthcare", Status = ContentStatus.Draft };
        var svc = new FakeFilterService([draft]);

        await Tools(svc).PublishContentAsync("filters", draft.Id.ToString(), CancellationToken.None);

        svc.LastSetStatus.Should().Be((draft.Id, ContentStatus.Published));
    }

    private static ContentTools Tools(IFilterService svc)
    {
        var services = new ServiceCollection();
        services.AddSingleton(svc);
        return new ContentTools(services.BuildServiceProvider());
    }

    private static JsonElement Json(string json) => JsonDocument.Parse(json).RootElement.Clone();
}
