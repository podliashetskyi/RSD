#pragma warning disable S1144, S4487, S2933

using RSD.Web.Data.Entities;
using RSD.Web.Services.Content;

namespace RSD.Web.Components.Sections.Services;

public partial class FeaturesSection(IServiceService Services)
{
    private IReadOnlyList<RSD.Web.Data.Entities.Service> Features { get; set; } = [];

    protected override async Task OnInitializedAsync()
    {
        var rows = await Services.ListAsync(new ContentQuery(Status: ContentStatus.Published, PageSize: 200), CancellationToken.None);
        Features = [.. rows.OrderByDescending(s => s.PublishedAt ?? s.CreatedAt)];
    }

    private static string DirectionClass(int index) =>
        index % 2 == 0 ? "lg:flex-row" : "lg:flex-row-reverse";
}
