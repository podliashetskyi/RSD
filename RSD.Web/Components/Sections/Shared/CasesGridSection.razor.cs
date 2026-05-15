#pragma warning disable S1144, S4487, S2933
using Microsoft.AspNetCore.Components;
using RSD.Web.Data.Entities;
using RSD.Web.Services.Content;

namespace RSD.Web.Components.Sections.Shared;

public partial class CasesGridSection(ICaseService Cases, IFilterService Filters)
{
    [Parameter] public bool ShowHeader        { get; set; } = true;
    [Parameter] public bool ShowFilters       { get; set; }
    [Parameter] public bool ShowViewAllButton { get; set; } = true;
    [Parameter] public int  MaxItems          { get; set; }

    private IReadOnlyList<Case> CaseList { get; set; } = [];

    private string? Industry { get; set; }
    private string? TechStack { get; set; }
    private int? Year { get; set; }
    private FilterKey? OpenFilter { get; set; }

    private IReadOnlyList<string> IndustryOptions { get; set; } = [];
    private IReadOnlyList<string> TechStackOptions { get; set; } = [];

    private IReadOnlyList<int> YearOptions =>
        [.. CaseList.Select(c => (c.PublishedAt ?? c.CreatedAt).Year).Distinct().OrderByDescending(y => y)];

    private bool HasAnyFilter => Industry is not null || TechStack is not null || Year is not null;

    private IReadOnlyList<Case> DisplayedCases
    {
        get
        {
            IEnumerable<Case> q = CaseList;
            if (Industry is { } ind) q = q.Where(c => string.Equals(c.Industry, ind, StringComparison.OrdinalIgnoreCase));
            if (TechStack is { } tech) q = q.Where(c => c.TechTags.Contains(tech, StringComparer.OrdinalIgnoreCase));
            if (Year is { } yr) q = q.Where(c => (c.PublishedAt ?? c.CreatedAt).Year == yr);
            var list = q.ToList();
            return MaxItems > 0 ? [.. list.Take(MaxItems)] : list;
        }
    }

    protected override async Task OnInitializedAsync()
    {
        var rows = await Cases.ListAsync(new ContentQuery(Status: ContentStatus.Published, PageSize: 200), CancellationToken.None);
        CaseList = [.. rows.OrderByDescending(c => c.PublishedAt ?? c.CreatedAt)];
        if (ShowFilters)
        {
            var industries = await Filters.ListByTypeAsync(FilterType.CaseIndustry, CancellationToken.None);
            var techTags = await Filters.ListByTypeAsync(FilterType.CaseTechTag, CancellationToken.None);
            IndustryOptions = [.. industries.Select(f => f.Label)];
            TechStackOptions = [.. techTags.Select(f => f.Label)];
        }
    }

    private void ToggleDropdown(FilterKey key) => OpenFilter = OpenFilter == key ? null : key;

    private void ApplyIndustry(string? value) { Industry = value; OpenFilter = null; }
    private void ApplyTechStack(string? value) { TechStack = value; OpenFilter = null; }
    private void ApplyYear(int? value) { Year = value; OpenFilter = null; }

    private void ClearAll()
    {
        Industry = null;
        TechStack = null;
        Year = null;
        OpenFilter = null;
    }

    private enum FilterKey { Industry, TechStack, Year }
}
