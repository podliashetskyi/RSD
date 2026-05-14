#pragma warning disable S1144, S4487, S2933
using Microsoft.AspNetCore.Components;
using RSD.Web.Data.Entities;
using RSD.Web.Services.Content;

namespace RSD.Web.Components.Sections.Shared;

public partial class CasesGridSection(ICaseService Cases)
{
    [Parameter] public bool ShowHeader        { get; set; } = true;
    [Parameter] public bool ShowFilters       { get; set; }
    [Parameter] public bool ShowViewAllButton { get; set; } = true;
    [Parameter] public int  MaxItems          { get; set; }

    private IReadOnlyList<Case> CaseList { get; set; } = [];
    private IReadOnlyList<Case> DisplayedCases =>
        MaxItems > 0 ? [.. CaseList.Take(MaxItems)] : CaseList;

    protected override async Task OnInitializedAsync()
    {
        var rows = await Cases.ListAsync(new ContentQuery(Status: ContentStatus.Published, PageSize: 200), CancellationToken.None);
        CaseList = [.. rows.OrderByDescending(c => c.PublishedAt ?? c.CreatedAt)];
    }

    private static readonly IReadOnlyList<FilterDropdown> Filters =
    [
        new FilterDropdown("Industry",     ["All", "Fintech", "Logistics", "Healthcare", "EdTech", "E-Commerce", "Industrial"]),
        new FilterDropdown("Tech Stack",   ["All", "React", "Python", "TypeScript", "Cloud", "AI/ML"]),
        new FilterDropdown("Project Type", ["All", "Web Platform", "Mobile App", "Cloud System"]),
        new FilterDropdown("Year",         ["All", "2025", "2024", "2023", "2022"]),
    ];
}

public record FilterDropdown(string Label, IReadOnlyList<string> Options);
