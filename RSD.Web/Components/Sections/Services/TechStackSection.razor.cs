#pragma warning disable S1144, S4487, S2933

namespace RSD.Web.Components.Sections.Services;

public partial class TechStackSection
{
    private static readonly IReadOnlyList<TechStackItem> Items =
    [
        new TechStackItem("​.NET", "dotnet"),
        new TechStackItem("C#", "csharp"),
        new TechStackItem("Azure", "azure"),
        new TechStackItem("SQL Server", "sql-server"),
        new TechStackItem("TypeScript", "typescript"),
        new TechStackItem("React", "react"),
        new TechStackItem("React Native", "react-native"),
        new TechStackItem("Flutter", "flutter"),
        new TechStackItem("Docker", "docker"),
        new TechStackItem("Kubernetes", "kubernetes"),
        new TechStackItem("PostgreSQL", "postgresql"),
        new TechStackItem("Redis", "redis"),
    ];
}

public record TechStackItem(string Label, string LogoBase);
