using RSD.Web.Data.Entities;
using RSD.Web.Services.Slugs;

namespace RSD.Web.Data.Seed;

public sealed class TechStackItemSeeder(AppDbContext Db, ISlugger Slugger) : SeederBase<TechStackItem>(Db, Slugger)
{
    protected override Task<IReadOnlyList<TechStackItem>> BuildAsync(CancellationToken ct)
    {
        IReadOnlyList<TechStackItem> items =
        [
            Build(".NET",         "dotnet",       1),
            Build("C#",           "csharp",       2),
            Build("Azure",        "azure",        3),
            Build("SQL Server",   "sql-server",   4),
            Build("TypeScript",   "typescript",   5),
            Build("React",        "react",        6),
            Build("React Native", "react-native", 7),
            Build("Flutter",      "flutter",      8),
            Build("Docker",       "docker",       9),
            Build("Kubernetes",   "kubernetes",   10),
            Build("PostgreSQL",   "postgresql",   11),
            Build("Redis",        "redis",        12),
        ];
        return Task.FromResult(items);
    }

    private static TechStackItem Build(string label, string logoBase, int order) => new()
    {
        Slug = label,
        Status = ContentStatus.Published,
        PublishedAt = DateTime.UtcNow,
        Label = label,
        LogoPath = $"images/services/tech/{logoBase}.png",
        DisplayOrder = order,
    };
}
