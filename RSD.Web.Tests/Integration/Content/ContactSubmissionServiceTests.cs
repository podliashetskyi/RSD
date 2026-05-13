using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using RSD.Web.Services.Content;
using RSD.Web.Tests.Integration.Fixtures;

namespace RSD.Web.Tests.Integration.Content;

[Collection(nameof(PostgresCollection))]
public sealed class ContactSubmissionServiceTests(PostgresFixture Postgres)
{
    [Fact]
    public async Task Submit_PersistsAndSendsEmail()
    {
        await using var factory = new AppDbContextFactory(Postgres.ConnectionString);
        await factory.CreateAsync();
        var service = factory.Provider.GetRequiredService<IContactSubmissionService>();
        var email = factory.Provider.GetRequiredService<CapturingEmailSender>();
        email.Sent.Clear();

        var marker = $"persist-{Guid.NewGuid():N}";
        var result = await service.SubmitAsync(new ContactSubmissionInput("Alice", $"{marker}@example.com", "Hello", "Body"), CancellationToken.None);

        result.Ok.Should().BeTrue();
        var stored = await service.GetByIdAsync(result.Value, CancellationToken.None);
        stored.Should().NotBeNull();
        stored!.Name.Should().Be("Alice");
        stored.Subject.Should().Be("Hello");
        stored.IsHandled.Should().BeFalse();

        // Notification is best-effort and fired without await — give it a tick.
        await WaitForAsync(() => email.Sent.Count > 0);
        email.Sent.Should().ContainSingle(m => m.Subject.Contains("Hello"));
    }

    [Fact]
    public async Task Submit_RejectsInvalidInput()
    {
        await using var factory = new AppDbContextFactory(Postgres.ConnectionString);
        await factory.CreateAsync();
        var service = factory.Provider.GetRequiredService<IContactSubmissionService>();

        var noName = await service.SubmitAsync(new ContactSubmissionInput("", "x@y.z", "s", "m"), CancellationToken.None);
        var noEmail = await service.SubmitAsync(new ContactSubmissionInput("n", "not-an-email", "s", "m"), CancellationToken.None);
        var noMessage = await service.SubmitAsync(new ContactSubmissionInput("n", "x@y.z", "s", ""), CancellationToken.None);

        noName.Ok.Should().BeFalse();
        noEmail.Ok.Should().BeFalse();
        noMessage.Ok.Should().BeFalse();
    }

    [Fact]
    public async Task MarkHandled_AndReopen_RoundTrip()
    {
        await using var factory = new AppDbContextFactory(Postgres.ConnectionString);
        await factory.CreateAsync();
        var service = factory.Provider.GetRequiredService<IContactSubmissionService>();

        var created = await service.SubmitAsync(new ContactSubmissionInput("Bob", "bob@example.com", "Q", "Body"), CancellationToken.None);
        (await service.MarkHandledAsync(created.Value, "user-1", CancellationToken.None)).Ok.Should().BeTrue();

        var handled = await service.GetByIdAsync(created.Value, CancellationToken.None);
        handled!.IsHandled.Should().BeTrue();
        handled.HandledByUserId.Should().Be("user-1");
        handled.HandledAt.Should().NotBeNull();

        (await service.MarkOpenAsync(created.Value, CancellationToken.None)).Ok.Should().BeTrue();
        var reopened = await service.GetByIdAsync(created.Value, CancellationToken.None);
        reopened!.IsHandled.Should().BeFalse();
        reopened.HandledByUserId.Should().Be("");
        reopened.HandledAt.Should().BeNull();
    }

    [Fact]
    public async Task List_FiltersAndPaginates()
    {
        await using var factory = new AppDbContextFactory(Postgres.ConnectionString);
        await factory.CreateAsync();
        var service = factory.Provider.GetRequiredService<IContactSubmissionService>();
        var marker = $"page-{Guid.NewGuid():N}";

        for (var i = 0; i < 30; i++)
        {
            await service.SubmitAsync(new ContactSubmissionInput($"User {i}", $"u{i}@x.io", $"{marker}-{i}", "Body"), CancellationToken.None);
        }

        var pageOne = await service.ListAsync(new ContactSubmissionQuery(Search: marker, Page: 1, PageSize: 10), CancellationToken.None);
        var pageTwo = await service.ListAsync(new ContactSubmissionQuery(Search: marker, Page: 2, PageSize: 10), CancellationToken.None);
        var pageFour = await service.ListAsync(new ContactSubmissionQuery(Search: marker, Page: 4, PageSize: 10), CancellationToken.None);

        pageOne.TotalCount.Should().Be(30);
        pageOne.Items.Should().HaveCount(10);
        pageTwo.Items.Should().HaveCount(10);
        pageFour.Items.Should().BeEmpty();
    }

    [Fact]
    public async Task Delete_RemovesRow()
    {
        await using var factory = new AppDbContextFactory(Postgres.ConnectionString);
        await factory.CreateAsync();
        var service = factory.Provider.GetRequiredService<IContactSubmissionService>();

        var created = await service.SubmitAsync(new ContactSubmissionInput("Carol", "c@x.io", "S", "M"), CancellationToken.None);
        (await service.DeleteAsync(created.Value, CancellationToken.None)).Ok.Should().BeTrue();
        var fetched = await service.GetByIdAsync(created.Value, CancellationToken.None);
        fetched.Should().BeNull();
    }

    private static async Task WaitForAsync(Func<bool> condition)
    {
        for (var i = 0; i < 50; i++)
        {
            if (condition()) return;
            await Task.Delay(20);
        }
    }
}
