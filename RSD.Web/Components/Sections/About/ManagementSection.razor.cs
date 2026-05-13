#pragma warning disable S1144, S4487, S2933

using RSD.Web.Data.Entities;
using RSD.Web.Services.Content;

namespace RSD.Web.Components.Sections.About;

public partial class ManagementSection(ITeamMemberService TeamService, ISocialLinkService SocialService)
{
    private IReadOnlyList<TeamMember> Managers { get; set; } = [];
    private IReadOnlyList<SocialLink> SocialIcons { get; set; } = [];

    protected override async Task OnInitializedAsync()
    {
        var team = await TeamService.ListAsync(new ContentQuery(Status: ContentStatus.Published, PageSize: 100), CancellationToken.None);
        Managers = team
            .Where(m => m.IsManagement)
            .OrderBy(m => m.DisplayOrder)
            .ToList();

        var socials = await SocialService.ListAsync(new ContentQuery(Status: ContentStatus.Published, PageSize: 100), CancellationToken.None);
        SocialIcons = socials
            .Where(s => s.Scope == SocialLinkScope.Management)
            .OrderBy(s => s.DisplayOrder)
            .ToList();
    }
}
