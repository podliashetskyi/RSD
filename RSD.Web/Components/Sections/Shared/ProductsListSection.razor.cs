#pragma warning disable S1144, S4487, S2933
using Microsoft.AspNetCore.Components;
using RSD.Web.Data.Entities;
using RSD.Web.Services.Content;

namespace RSD.Web.Components.Sections.Shared;

public partial class ProductsListSection(IProductService Products)
{
    [Parameter] public bool ShowHeader        { get; set; } = true;
    [Parameter] public bool ShowViewAllButton { get; set; } = true;

    private IReadOnlyList<Product> Items { get; set; } = [];

    protected override async Task OnInitializedAsync()
    {
        var rows = await Products.ListAsync(new ContentQuery(Status: ContentStatus.Published, PageSize: 200), CancellationToken.None);
        Items = [.. rows.OrderByDescending(p => p.PublishedAt ?? p.CreatedAt)];
    }

    private static string DirectionClass(int index) =>
        index % 2 == 0 ? "lg:flex-row" : "lg:flex-row-reverse";
}
