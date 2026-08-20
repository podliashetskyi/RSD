using Bunit;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using RSD.Web.Components.Sections.Detail;
using RSD.Web.Components.Sections.Shared;
using RSD.Web.Data.Entities;
using RSD.Web.Services.Content;

namespace RSD.Web.Tests.Unit.Components;

public sealed class RelatedAndTechPillTests
{
    [Fact]
    public void TechPills_RenderAsLinks_ToTheFilteredCasesGrid()
    {
        using var ctx = new BunitContext();

        var cut = ctx.Render<TechPillsCard>(p => p
            .Add(x => x.Heading, "Technology")
            .Add(x => x.Pills, new List<string> { "React", ".NET Core" }));

        var links = cut.FindAll("a");
        links.Select(a => a.GetAttribute("href")).Should().Contain("/cases?tech=React");
        links.Select(a => a.GetAttribute("href")).Should().Contain("/cases?tech=.NET%20Core");
    }

    [Fact]
    public void RelatedContentSection_RendersLinkedCards_AndNothingWhenEmpty()
    {
        using var ctx = new BunitContext();

        var cut = ctx.Render<RelatedContentSection>(p => p
            .Add(x => x.Heading, "Related case studies")
            .Add(x => x.Items, new List<RelatedLink>
            {
                new("Healthcare", "Healthcare Plus", "/cases/healthcare-plus"),
                new("Fintech", "PayFlow", "/cases/payflow"),
            }));

        cut.Markup.Should().Contain("Related case studies");
        var links = cut.FindAll("article a").Select(a => a.GetAttribute("href")).ToList();
        links.Should().Contain("/cases/healthcare-plus").And.Contain("/cases/payflow");
        cut.FindAll("article h3").Should().HaveCount(2);

        var empty = ctx.Render<RelatedContentSection>(p => p
            .Add(x => x.Heading, "Related")
            .Add(x => x.Items, new List<RelatedLink>()));
        empty.Markup.Trim().Should().BeEmpty();
    }

    [Fact]
    public void CasesGrid_AppliesInitialTechFilter_FromQuery()
    {
        using var ctx = new BunitContext();
        ctx.JSInterop.Mode = JSRuntimeMode.Loose;
        ctx.Services.AddSingleton<ICaseService>(new FakeCaseService(
        [
            new Case { Slug = "react-case", Name = "React Case", TechTags = ["React"], Status = ContentStatus.Published },
            new Case { Slug = "iot-case", Name = "IoT Case", TechTags = ["IoT"], Status = ContentStatus.Published },
        ]));
        ctx.Services.AddSingleton<IFilterService>(new FakeFilterService());

        var cut = ctx.Render<CasesGridSection>(p => p.Add(x => x.InitialTech, "React"));

        cut.Markup.Should().Contain("React Case");
        cut.Markup.Should().NotContain("IoT Case");
    }
}

internal sealed class FakeCaseService(IReadOnlyList<Case> cases) : ICaseService
{
    public Task<IReadOnlyList<Case>> ListAsync(ContentQuery query, CancellationToken ct) => Task.FromResult(cases);
    public Task<Case?> GetByIdAsync(Guid id, CancellationToken ct) => Task.FromResult(cases.FirstOrDefault(c => c.Id == id));
    public Task<Case?> GetBySlugAsync(string slug, bool includeDrafts, CancellationToken ct) =>
        Task.FromResult(cases.FirstOrDefault(c => c.Slug == slug && (includeDrafts || c.Status == ContentStatus.Published)));
    public Task<RSD.Web.Services.Common.Result<Guid>> CreateAsync(CaseUpsert input, CancellationToken ct) => throw new NotImplementedException();
    public Task<RSD.Web.Services.Common.Result<RSD.Web.Services.Common.Unit>> UpdateAsync(Guid id, CaseUpsert input, CancellationToken ct) => throw new NotImplementedException();
    public Task<RSD.Web.Services.Common.Result<RSD.Web.Services.Common.Unit>> PublishAsync(Guid id, CancellationToken ct) => throw new NotImplementedException();
    public Task<RSD.Web.Services.Common.Result<RSD.Web.Services.Common.Unit>> UnpublishAsync(Guid id, CancellationToken ct) => throw new NotImplementedException();
    public Task<RSD.Web.Services.Common.Result<RSD.Web.Services.Common.Unit>> ArchiveAsync(Guid id, CancellationToken ct) => throw new NotImplementedException();
    public Task<RSD.Web.Services.Common.Result<RSD.Web.Services.Common.Unit>> SoftDeleteAsync(Guid id, CancellationToken ct) => throw new NotImplementedException();
    public Task<RSD.Web.Services.Common.Result<RSD.Web.Services.Common.Unit>> RestoreAsync(Guid id, CancellationToken ct) => throw new NotImplementedException();
    public Task<RSD.Web.Services.Common.Result<RSD.Web.Services.Common.Unit>> HardDeleteAsync(Guid id, CancellationToken ct) => throw new NotImplementedException();
}
