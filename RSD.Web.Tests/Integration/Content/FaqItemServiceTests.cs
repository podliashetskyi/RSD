using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using RSD.Web.Data.Entities;
using RSD.Web.Services.Content;
using RSD.Web.Services.Content.Trash;
using RSD.Web.Tests.Integration.Fixtures;

namespace RSD.Web.Tests.Integration.Content;

/// <summary>
/// FAQ answers are admin-authored HTML that other writers (seeders, future MCP tools)
/// also produce — so sanitization must live in the service, not in the admin page.
/// </summary>
[Collection(nameof(PostgresCollection))]
public sealed class FaqItemServiceTests(PostgresFixture Postgres)
{
    [Fact]
    public async Task CreateAsync_SanitizesAnswerHtml()
    {
        await using var factory = new AppDbContextFactory(Postgres.ConnectionString);
        var db = await factory.CreateAsync();
        var service = factory.Provider.GetRequiredService<IFaqItemService>();

        var result = await service.CreateAsync(Faq("<p>Fine.</p><script>alert(1)</script><iframe src=x></iframe>"), CancellationToken.None);

        result.Ok.Should().BeTrue(result.Error);
        var stored = (await db.FaqItems.AsNoTracking().FirstAsync(f => f.Id == result.Value)).AnswerHtml;
        stored.Should().Contain("<p>Fine.</p>");
        stored.Should().NotContain("script").And.NotContain("iframe");
    }

    [Fact]
    public async Task UpdateAsync_SanitizesAnswerHtml()
    {
        await using var factory = new AppDbContextFactory(Postgres.ConnectionString);
        var db = await factory.CreateAsync();
        var service = factory.Provider.GetRequiredService<IFaqItemService>();
        var created = await service.CreateAsync(Faq("<p>Original</p>"), CancellationToken.None);
        var entity = await db.FaqItems.AsNoTracking().FirstAsync(f => f.Id == created.Value);

        entity.AnswerHtml = "<p>Edited</p><script>steal()</script>";
        var updated = await service.UpdateAsync(entity, CancellationToken.None);

        updated.Ok.Should().BeTrue(updated.Error);
        var stored = (await db.FaqItems.AsNoTracking().FirstAsync(f => f.Id == created.Value)).AnswerHtml;
        stored.Should().Contain("<p>Edited</p>").And.NotContain("script");
    }

    [Fact]
    public async Task Trash_ListsAndRestores_FaqItems()
    {
        await using var factory = new AppDbContextFactory(Postgres.ConnectionString);
        await factory.CreateAsync();
        var service = factory.Provider.GetRequiredService<IFaqItemService>();
        var trash = factory.Provider.GetRequiredService<ITrashService>();
        var created = await service.CreateAsync(Faq("<p>Trash me.</p>"), CancellationToken.None);

        (await service.SoftDeleteAsync(created.Value, CancellationToken.None)).Ok.Should().BeTrue();
        var listed = await trash.ListAsync(CancellationToken.None);
        listed.Should().Contain(i => i.EntityKey == "faq" && i.Id == created.Value);

        (await trash.RestoreAsync("faq", created.Value, CancellationToken.None)).Ok.Should().BeTrue();
        var relisted = await trash.ListAsync(CancellationToken.None);
        relisted.Should().NotContain(i => i.Id == created.Value);
    }

    private static FaqItem Faq(string answerHtml) => new()
    {
        Slug = "",
        Question = $"Q {Guid.NewGuid():N}?",
        AnswerHtml = answerHtml,
        Status = ContentStatus.Published,
    };
}
