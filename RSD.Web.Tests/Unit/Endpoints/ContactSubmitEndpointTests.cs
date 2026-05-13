using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using RSD.Web.Data.Entities;
using RSD.Web.Endpoints;
using RSD.Web.Services.Common;
using RSD.Web.Services.Content;

namespace RSD.Web.Tests.Unit.Endpoints;

public sealed class ContactSubmitEndpointTests
{
    [Fact]
    public async Task Honeypot_ShortCircuits_WithoutCallingService()
    {
        var fake = new FakeService();
        var request = new ContactSubmitRequest { Name = "Bot", Email = "bot@x.io", Subject = "S", Message = "M", Hp = "spam" };

        var result = await ContactSubmitEndpoint.HandleAsync(request, fake, CancellationToken.None);

        result.Should().BeOfType<Ok>();
        fake.Calls.Should().Be(0);
    }

    [Fact]
    public async Task ValidSubmission_DelegatesToService_AndReturnsOk()
    {
        var fake = new FakeService();
        var request = new ContactSubmitRequest { Name = "Alice", Email = "a@x.io", Subject = "Hi", Message = "Hello" };

        var result = await ContactSubmitEndpoint.HandleAsync(request, fake, CancellationToken.None);

        result.Should().BeOfType<Ok>();
        fake.Calls.Should().Be(1);
        fake.LastInput!.Name.Should().Be("Alice");
        fake.LastInput.Email.Should().Be("a@x.io");
    }

    [Fact]
    public async Task ServiceFailure_ReturnsBadRequest()
    {
        var fake = new FakeService { FailWith = "Invalid" };
        var request = new ContactSubmitRequest { Name = "Alice", Email = "a@x.io", Subject = "Hi", Message = "Hello" };

        var result = await ContactSubmitEndpoint.HandleAsync(request, fake, CancellationToken.None);

        var badRequest = result as IStatusCodeHttpResult;
        badRequest.Should().NotBeNull();
        badRequest!.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
    }

    private sealed class FakeService : IContactSubmissionService
    {
        public int Calls;
        public ContactSubmissionInput? LastInput;
        public string FailWith { get; init; } = "";

        public Task<Result<Guid>> SubmitAsync(ContactSubmissionInput input, CancellationToken ct)
        {
            Calls++;
            LastInput = input;
            return Task.FromResult(string.IsNullOrEmpty(FailWith)
                ? Result.Ok(Guid.NewGuid())
                : Result.Fail<Guid>(FailWith));
        }

        public Task<ContactSubmissionPage> ListAsync(ContactSubmissionQuery query, CancellationToken ct) =>
            Task.FromResult(new ContactSubmissionPage([], 0));

        public Task<ContactSubmission?> GetByIdAsync(Guid id, CancellationToken ct) =>
            Task.FromResult<ContactSubmission?>(null);

        public Task<Result<RSD.Web.Services.Common.Unit>> MarkHandledAsync(Guid id, string userId, CancellationToken ct) =>
            Task.FromResult(Result.Ok());

        public Task<Result<RSD.Web.Services.Common.Unit>> MarkOpenAsync(Guid id, CancellationToken ct) =>
            Task.FromResult(Result.Ok());

        public Task<Result<RSD.Web.Services.Common.Unit>> DeleteAsync(Guid id, CancellationToken ct) =>
            Task.FromResult(Result.Ok());
    }
}
