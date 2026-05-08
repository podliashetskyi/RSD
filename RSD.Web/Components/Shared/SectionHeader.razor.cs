#pragma warning disable S1144, S4487, S2933
using Microsoft.AspNetCore.Components;

namespace RSD.Web.Components.Shared;

public partial class SectionHeader
{
    [Parameter] public string BadgeLabel { get; set; } = "";
    [Parameter] public string Heading { get; set; } = "";
    [Parameter] public string Description { get; set; } = "";
    [Parameter] public SectionHeaderTone Tone { get; set; } = SectionHeaderTone.Light;

    private const string HeadingBase = "text-h2 font-semibold tracking-tight text-center leading-none";
    private const string DescriptionBase = "text-body-lg text-center max-w-2xl";

    private string HeadingClass => Tone == SectionHeaderTone.Dark
        ? $"{HeadingBase} text-white"
        : $"{HeadingBase} text-ink";

    private string DescriptionClass => Tone == SectionHeaderTone.Dark
        ? $"{DescriptionBase} text-ink-muted"
        : $"{DescriptionBase} text-ink-soft";
}

public enum SectionHeaderTone
{
    Light,
    Dark,
}
