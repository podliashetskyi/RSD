#pragma warning disable S1144, S4487, S2933
using System.Collections.Generic;

namespace RSD.Web.Components.Sections.About;

public partial class PartnersSection
{
    private static readonly IReadOnlyList<PartnerCard> Partners =
    [
        new("images/about/partners/portrait-bonnie-green.png",  "Bonnie Green",  "Front-end Developer", "/contact"),
        new("images/about/partners/portrait-bonnie-green.png",  "Robert Fox",    "Front-end Developer", "/contact"),
        new("images/about/partners/portrait-eleanor-pena.png",  "Eleanor Pena",  "Front-end Developer", "/contact"),
        new("images/about/partners/portrait-esther-howard.png", "Esther Howard", "Front-end Developer", "/contact"),
    ];
}

public record PartnerCard(string PhotoSrc, string Name, string Role, string ContactHref);
