#pragma warning disable S1144, S4487, S2933

using RSD.Web.Data.Entities;
using RSD.Web.Services.Content;

namespace RSD.Web.Components.Sections.About;

public partial class TeamSection(ITeamMemberService Service)
{
    private IReadOnlyList<TeamMember> TeamRow1 { get; set; } = [];
    private IReadOnlyList<TeamMember> TeamRow2 { get; set; } = [];

    protected override async Task OnInitializedAsync()
    {
        var list = await Service.ListAsync(new ContentQuery(Status: ContentStatus.Published, PageSize: 100), CancellationToken.None);
        var members = list
            .Where(m => !m.IsManagement)
            .OrderBy(m => m.DisplayOrder)
            .ToList();
        TeamRow1 = members.Take(6).ToList();
        TeamRow2 = members.Skip(6).ToList();
    }
}
