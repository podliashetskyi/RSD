#pragma warning disable S1144, S4487, S2933

using RSD.Web.Data.Entities;
using RSD.Web.Services.Content;

namespace RSD.Web.Components.Sections.Services;

public partial class TechStackSection(ITechStackItemService Service)
{
    private IReadOnlyList<TechStackItem> Items { get; set; } = [];

    protected override async Task OnInitializedAsync()
    {
        var list = await Service.ListAsync(new ContentQuery(Status: ContentStatus.Published, PageSize: 100), CancellationToken.None);
        Items = list.OrderBy(t => t.DisplayOrder).ToList();
    }
}
