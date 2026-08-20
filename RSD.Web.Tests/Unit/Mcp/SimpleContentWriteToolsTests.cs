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
/// Simple-type writes: the entity is the payload (slug "" = auto-generate); create forces
/// Draft, update requires the echoed id and pins the existing status.
/// </summary>
public sealed class SimpleContentWriteToolsTests
{
    [Fact]
    public async Task Create_ForcesDraft_ForSimpleTypes()
    {
        var svc = new FakeFaqItemService([]);
        await Tools(svc).CreateContentAsync("faq",
            Json("""{"slug":"","question":"Q?","answerHtml":"<p>A</p>","status":"Published"}"""), CancellationToken.None);

        svc.LastCreate!.Status.Should().Be(ContentStatus.Draft);
        svc.LastCreate.As<FaqItem>().Question.Should().Be("Q?");
    }

    [Fact]
    public async Task Create_MissingRequiredMember_GivesAClearParseError()
    {
        var act = () => Tools(new FakeFaqItemService([])).CreateContentAsync("faq",
            Json("""{"slug":""}"""), CancellationToken.None);

        await act.Should().ThrowAsync<McpException>().WithMessage("*question*");
    }

    [Fact]
    public async Task Update_RequiresTheEchoedId_ToMatchTheTarget()
    {
        var existing = new FaqItem { Slug = "q", Question = "Q?", Status = ContentStatus.Draft };
        var svc = new FakeFaqItemService([existing]);

        var act = () => Tools(svc).UpdateContentAsync("faq", existing.Id.ToString(),
            Json($$"""{"id":"{{Guid.NewGuid()}}","slug":"q","question":"Changed?"}"""), allowLiveEdit: false, CancellationToken.None);

        await act.Should().ThrowAsync<McpException>().WithMessage("*id*");
        svc.LastUpdate.Should().BeNull();
    }

    [Fact]
    public async Task Update_PinsExistingStatus_AndRefusesPublishedWithoutFlag()
    {
        var live = new FaqItem { Slug = "live", Question = "Live?", Status = ContentStatus.Published };
        var svc = new FakeFaqItemService([live]);

        var refused = () => Tools(svc).UpdateContentAsync("faq", live.Id.ToString(),
            Json($$"""{"id":"{{live.Id}}","slug":"live","question":"Changed?"}"""), allowLiveEdit: false, CancellationToken.None);
        await refused.Should().ThrowAsync<McpException>().WithMessage("*allowLiveEdit*");

        await Tools(svc).UpdateContentAsync("faq", live.Id.ToString(),
            Json($$"""{"id":"{{live.Id}}","slug":"live","question":"Changed?","status":"Draft"}"""), allowLiveEdit: true, CancellationToken.None);

        svc.LastUpdate!.Status.Should().Be(ContentStatus.Published);
        svc.LastUpdate.As<FaqItem>().Question.Should().Be("Changed?");
    }

    private static ContentTools Tools(IFaqItemService svc)
    {
        var services = new ServiceCollection();
        services.AddSingleton(svc);
        return new ContentTools(services.BuildServiceProvider());
    }

    private static JsonElement Json(string json) => JsonDocument.Parse(json).RootElement.Clone();
}
