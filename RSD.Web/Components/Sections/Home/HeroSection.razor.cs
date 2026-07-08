#pragma warning disable S1144, S4487, S2933

using RSD.Web.Data.Entities;
using RSD.Web.Services.Content;

namespace RSD.Web.Components.Sections.Home;

public partial class HeroSection(IMissionStatService Service)
{
    private IReadOnlyList<MissionStat> Stats { get; set; } = [];

    protected override async Task OnInitializedAsync()
    {
        var list = await Service.ListAsync(
            new ContentQuery(Status: ContentStatus.Published, PageSize: 100), CancellationToken.None);
        Stats = list.OrderBy(s => s.DisplayOrder).Take(3).ToList();
    }
}
