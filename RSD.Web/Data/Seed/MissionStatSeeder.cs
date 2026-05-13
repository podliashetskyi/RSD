using RSD.Web.Data.Entities;
using RSD.Web.Services.Slugs;

namespace RSD.Web.Data.Seed;

public sealed class MissionStatSeeder(AppDbContext Db, ISlugger Slugger) : SeederBase<MissionStat>(Db, Slugger)
{
    protected override Task<IReadOnlyList<MissionStat>> BuildAsync(CancellationToken ct)
    {
        IReadOnlyList<MissionStat> items =
        [
            Build("8",   "+", "Years of Experience", 1),
            Build("200", "+", "Projects",            2),
            Build("50",  "+", "Partners",            3),
            Build("60",  "+", "Developers",          4),
        ];
        return Task.FromResult(items);
    }

    private static MissionStat Build(string number, string symbol, string label, int order) => new()
    {
        Slug = label,
        Status = ContentStatus.Published,
        PublishedAt = DateTime.UtcNow,
        Number = number,
        Symbol = symbol,
        Label = label,
        DisplayOrder = order,
    };
}
