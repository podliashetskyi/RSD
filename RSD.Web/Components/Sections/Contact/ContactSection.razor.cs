#pragma warning disable S1144, S4487, S2933

using RSD.Web.Data.Entities;
using RSD.Web.Services.Content;

namespace RSD.Web.Components.Sections.Contact;

public partial class ContactSection(
    IContactPointService PointsService,
    IMessengerLinkService MessengerService,
    ISocialLinkService SocialService)
{
    private IReadOnlyList<ContactPoint> Points { get; set; } = [];
    private IReadOnlyList<MessengerLink> Messengers { get; set; } = [];
    private IReadOnlyList<SocialLink> Socials { get; set; } = [];

    protected override async Task OnInitializedAsync()
    {
        var pts = await PointsService.ListAsync(new ContentQuery(Status: ContentStatus.Published, PageSize: 100), CancellationToken.None);
        Points = pts.OrderBy(p => p.DisplayOrder).ToList();

        var msgs = await MessengerService.ListAsync(new ContentQuery(Status: ContentStatus.Published, PageSize: 100), CancellationToken.None);
        Messengers = msgs.OrderBy(m => m.DisplayOrder).ToList();

        var socials = await SocialService.ListAsync(new ContentQuery(Status: ContentStatus.Published, PageSize: 100), CancellationToken.None);
        Socials = socials.Where(s => s.Scope == SocialLinkScope.Contact).OrderBy(s => s.DisplayOrder).ToList();
    }
}
