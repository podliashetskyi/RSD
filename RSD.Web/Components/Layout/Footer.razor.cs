#pragma warning disable S1144, S4487, S2933

using Microsoft.AspNetCore.Components;
using RSD.Web.Data.Entities;
using RSD.Web.Services.Content;

namespace RSD.Web.Components.Layout;

public partial class Footer(ISocialLinkService Service) : ComponentBase
{
    private IReadOnlyList<SocialLink> Socials { get; set; } = [];

    protected override async Task OnInitializedAsync()
    {
        var list = await Service.ListAsync(new ContentQuery(Status: ContentStatus.Published, PageSize: 100), CancellationToken.None);
        Socials = list.Where(s => s.Scope == SocialLinkScope.Footer).OrderBy(s => s.DisplayOrder).ToList();
    }
}
