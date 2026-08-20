using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using RSD.Web.Data.Entities;
using RSD.Web.Services.Content;
using RSD.Web.Services.Mcp.Tools;
using RSD.Web.Tests.Unit.Components;

namespace RSD.Web.Tests.Unit.Mcp;

/// <summary>publish_content is the single deliberate path from Draft to live.</summary>
public sealed class PublishToolTests
{
    [Fact]
    public async Task Publish_SimpleType_SetsStatusPublished()
    {
        var item = new FaqItem { Slug = "q", Question = "Q?", Status = ContentStatus.Draft };
        var svc = new FakeFaqItemService([item]);
        var services = new ServiceCollection();
        services.AddSingleton<IFaqItemService>(svc);
        var tools = new ContentTools(services.BuildServiceProvider());

        await tools.PublishContentAsync("faq", item.Id.ToString(), CancellationToken.None);

        svc.LastSetStatus.Should().Be((item.Id, ContentStatus.Published));
    }
}
