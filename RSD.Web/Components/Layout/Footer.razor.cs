#pragma warning disable S1144, S4487, S2933

using Microsoft.AspNetCore.Components;
using RSD.Web.Data.Entities;
using RSD.Web.Services.Content;

namespace RSD.Web.Components.Layout;

public partial class Footer(ISocialLinkService SocialService, IContactPointService PointsService) : ComponentBase
{
    private IReadOnlyList<SocialLink> Socials { get; set; } = [];
    private IReadOnlyList<ContactPoint> Points { get; set; } = [];

    protected override async Task OnInitializedAsync()
    {
        var socials = await SocialService.ListAsync(new ContentQuery(Status: ContentStatus.Published, PageSize: 100), CancellationToken.None);
        Socials = socials.Where(HasPublicFooterHref)
                         .OrderBy(s => s.DisplayOrder)
                         .ToList();

        var pts = await PointsService.ListAsync(new ContentQuery(Status: ContentStatus.Published, PageSize: 100), CancellationToken.None);
        Points = pts.OrderBy(p => p.DisplayOrder).ToList();
    }

    private static bool HasPublicFooterHref(SocialLink link) =>
        link.Scope == SocialLinkScope.Footer
        && LinkHrefValidator.IsValidSocialHref(link.Href)
        && !string.IsNullOrWhiteSpace(link.Href);
}
