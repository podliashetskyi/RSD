#pragma warning disable S1144, S4487, S2933
using Microsoft.AspNetCore.Components;
using RSD.Web.Data.Entities;
using RSD.Web.Services.Content;
using RSD.Web.Services.Seo;

namespace RSD.Web.Components.Sections.Shared;

public partial class FaqSection(IFaqItemService Service)
{
    /// <summary>Home mode: only admin-pinned items (ShowOnHome), capped at four.</summary>
    [Parameter] public bool PinnedOnly { get; set; }

    private IReadOnlyList<FaqItem> Items { get; set; } = [];

    private string FaqJson => FaqJsonLdBuilder.Build(Items);

    private const int HomeCap = 4;

    protected override async Task OnInitializedAsync()
    {
        var list = await Service.ListAsync(new ContentQuery(Status: ContentStatus.Published, PageSize: 100), CancellationToken.None);
        var sitewide = list.Where(f => f.OwnerSlug.Length == 0);
        Items = PinnedOnly
            ? [.. sitewide.Where(f => f.ShowOnHome).OrderBy(f => f.DisplayOrder).Take(HomeCap)]
            : [.. sitewide.OrderByDescending(f => f.ShowOnHome).ThenBy(f => f.DisplayOrder)];
    }
}
