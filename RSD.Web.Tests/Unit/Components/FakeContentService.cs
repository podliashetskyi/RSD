using RSD.Web.Data.Entities;
using RSD.Web.Services.Common;
using RSD.Web.Services.Content;

// The namespace segment "Unit" (RSD.Web.Tests.Unit) shadows the Unit struct by simple name,
// so alias it to a distinct, fully-rooted name for use in the interface return types.
using CommonUnit = global::RSD.Web.Services.Common.Unit;

namespace RSD.Web.Tests.Unit.Components;

// Marker interfaces (IMissionStatService etc.) add no members, so one base fake
// implementing ISimpleContentService<T> serves every content type.
public class FakeContentService<T>(IReadOnlyList<T> items) : ISimpleContentService<T>
    where T : ContentEntity
{
    public Task<IReadOnlyList<T>> ListAsync(ContentQuery query, CancellationToken ct) => Task.FromResult(items);
    public Task<T?> GetByIdAsync(Guid id, CancellationToken ct) => Task.FromResult(items.FirstOrDefault(i => i.Id == id));
    public Task<Result<Guid>> CreateAsync(T input, CancellationToken ct) => Task.FromResult(Result.Ok(input.Id));
    public Task<Result<CommonUnit>> UpdateAsync(T input, CancellationToken ct) => Task.FromResult(Result.Ok());
    public Task<Result<CommonUnit>> SetStatusAsync(Guid id, ContentStatus status, CancellationToken ct) => throw new NotImplementedException();
    public Task<Result<CommonUnit>> SoftDeleteAsync(Guid id, CancellationToken ct) => throw new NotImplementedException();
    public Task<Result<CommonUnit>> RestoreAsync(Guid id, CancellationToken ct) => throw new NotImplementedException();
    public Task<Result<CommonUnit>> HardDeleteAsync(Guid id, CancellationToken ct) => throw new NotImplementedException();
    public Task<Result<CommonUnit>> BulkReorderAsync(IReadOnlyList<ReorderEntry> ordered, CancellationToken ct) => throw new NotImplementedException();
}

public sealed class FakeMissionStatService(IReadOnlyList<MissionStat> items)
    : FakeContentService<MissionStat>(items), IMissionStatService;

public sealed class FakeContactPointService(IReadOnlyList<ContactPoint> items)
    : FakeContentService<ContactPoint>(items), IContactPointService;

public sealed class FakeSocialLinkService(IReadOnlyList<SocialLink> items)
    : FakeContentService<SocialLink>(items), ISocialLinkService;
