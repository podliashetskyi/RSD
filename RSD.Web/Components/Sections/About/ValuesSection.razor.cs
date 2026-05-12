#pragma warning disable S1144, S4487, S2933
using System.Collections.Generic;

namespace RSD.Web.Components.Sections.About;

public partial class ValuesSection
{
    private static readonly IReadOnlyList<ValueEntry> Values =
    [
        new("images/about/values/icon-rocket.svg",    "Results",     "We focus on outcomes for clients, not just tasks."),
        new("images/about/values/icon-lightbulb.svg", "Innovation",  "Exploring new technologies and best practices."),
        new("images/about/values/icon-heart.svg",     "Partnership", "Building long-term relationships based on trust and mutual respect."),
        new("images/about/values/icon-users.svg",     "Team",        "Investing in our team's development — our main asset."),
    ];
}

public record ValueEntry(string IconSrc, string Title, string Description);
