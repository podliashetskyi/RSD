#pragma warning disable S1144, S4487, S2933

using RSD.Web.Data.Entities;
using RSD.Web.Services.Content;

namespace RSD.Web.Components.Sections.About;

public partial class ManagementSection(ITeamMemberService TeamService)
{
    private IReadOnlyList<TeamMember> Managers { get; set; } = [];

    protected override async Task OnInitializedAsync()
    {
        var team = await TeamService.ListAsync(new ContentQuery(Status: ContentStatus.Published, PageSize: 100), CancellationToken.None);
        Managers = [.. team.Where(m => m.IsManagement).OrderBy(m => m.DisplayOrder)];
    }

    private static IEnumerable<SocialIcon> IconsFor(TeamMember m)
    {
        if (!string.IsNullOrWhiteSpace(m.LinkedInUrl))
            yield return new("LinkedIn", $"{m.Name} on LinkedIn", "images/about/social/icon-linkedin.svg", m.LinkedInUrl);
        if (!string.IsNullOrWhiteSpace(m.XUrl))
            yield return new("X", $"{m.Name} on X", "images/about/social/icon-x.svg", m.XUrl);
        if (!string.IsNullOrWhiteSpace(m.GitHubUrl))
            yield return new("GitHub", $"{m.Name} on GitHub", "images/about/social/icon-github.svg", m.GitHubUrl);
        if (!string.IsNullOrWhiteSpace(m.Email))
            yield return new("Email", $"Email {m.Name}", "images/about/social/icon-envelope.svg", $"mailto:{m.Email}");
    }

    private sealed record SocialIcon(string Platform, string Label, string IconPath, string Href);
}
