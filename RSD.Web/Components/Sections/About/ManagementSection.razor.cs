#pragma warning disable S1144, S4487, S2933
using System.Collections.Generic;

namespace RSD.Web.Components.Sections.About;

public partial class ManagementSection
{
    private static readonly IReadOnlyList<ManagerCard> Managers =
    [
        new("images/about/management/portrait-bonnie-green.png",   "Bonnie Green",  "Front-end Developer"),
        new("images/about/management/portrait-robert-fox.png",     "Robert Fox",    "Front-end Developer"),
        new("images/about/management/portrait-eleanor-pena.png",   "Eleanor Pena",  "Front-end Developer"),
        new("images/about/management/portrait-esther-howard.png",  "Esther Howard", "Front-end Developer"),
    ];

    private static readonly IReadOnlyList<SocialIcon> SocialIcons =
    [
        new("images/about/social/icon-x.svg",        "X"),
        new("images/about/social/icon-google.svg",   "Google"),
        new("images/about/social/icon-github.svg",   "GitHub"),
        new("images/about/social/icon-dribbble.svg", "Dribbble"),
        new("images/about/social/icon-linkedin.svg", "LinkedIn"),
    ];
}

public record ManagerCard(string PhotoSrc, string Name, string Role);
public record SocialIcon(string Src, string Label);
