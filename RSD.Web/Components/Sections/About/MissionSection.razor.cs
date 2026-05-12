#pragma warning disable S1144, S4487, S2933
using System.Collections.Generic;

namespace RSD.Web.Components.Sections.About;

public partial class MissionSection
{
    private static readonly IReadOnlyList<StatEntry> Stats =
    [
        new("8", "+", "Years of Experience"),
        new("200", "+", "Projects"),
        new("50", "+", "Partners"),
        new("60", "+", "Developers"),
    ];
}

public record StatEntry(string Number, string Symbol, string Label);
