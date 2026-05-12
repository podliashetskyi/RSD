#pragma warning disable S1144, S4487, S2933
using System.Collections.Generic;

namespace RSD.Web.Components.Sections.About;

public partial class TeamSection
{
    private static readonly IReadOnlyList<TeamMember> TeamRow1 =
    [
        new("images/about/team/avatar-01.png", "Floyd Miles",     "Front-End Engineer"),
        new("images/about/team/avatar-02.png", "Ralph Edwards",   "Front-End Engineer"),
        new("images/about/team/avatar-03.png", "Kathryn Murphy",  "Back-End Engineer"),
        new("images/about/team/avatar-04.png", "Robert Fox",      "Back-End Engineer"),
        new("images/about/team/avatar-05.png", "Kathryn Murphy",  "Back-End Engineer"),
        new("images/about/team/avatar-06.png", "Robert Fox",      "Back-End Engineer"),
    ];

    private static readonly IReadOnlyList<TeamMember> TeamRow2 =
    [
        new("images/about/team/avatar-07.png", "Floyd Miles",     "Front-End Engineer"),
        new("images/about/team/avatar-08.png", "Ralph Edwards",   "Front-End Engineer"),
        new("images/about/team/avatar-09.png", "Kathryn Murphy",  "Back-End Engineer"),
        new("images/about/team/avatar-10.png", "Robert Fox",      "Back-End Engineer"),
        new("images/about/team/avatar-11.png", "Kathryn Murphy",  "Back-End Engineer"),
    ];
}

public record TeamMember(string AvatarSrc, string Name, string Role);
