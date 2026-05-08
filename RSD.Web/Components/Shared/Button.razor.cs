#pragma warning disable S1144, S4487, S2933
using Microsoft.AspNetCore.Components;

namespace RSD.Web.Components.Shared;

public partial class Button
{
    [Parameter] public RenderFragment? ChildContent { get; set; }
    [Parameter] public string? Href { get; set; }
    [Parameter] public ButtonVariant Variant { get; set; } = ButtonVariant.Primary;
    [Parameter] public ButtonSize Size { get; set; } = ButtonSize.Md;
    [Parameter] public bool TrailingArrow { get; set; }
    [Parameter] public string? AdditionalClasses { get; set; }
    [Parameter] public EventCallback OnClick { get; set; }

    private const string Base = "inline-flex items-center justify-center gap-2 font-medium rounded-full transition-colors";

    private string FullClass => $"{Base} {SizeClass(Size)} {VariantClass(Variant)} {AdditionalClasses}".Trim();

    private static string SizeClass(ButtonSize size) => size switch
    {
        ButtonSize.Lg => "px-5 py-3.5 text-body",
        ButtonSize.Md => "px-4 py-2.5 text-body",
        _             => "px-4 py-2.5 text-body",
    };

    private static string VariantClass(ButtonVariant variant) => variant switch
    {
        ButtonVariant.Primary => "text-white bg-brand-950 hover:bg-brand-900 shadow-sm",
        ButtonVariant.Outline => "text-ink-soft bg-white border border-cyan-600 hover:bg-line-subtle shadow-sm",
        ButtonVariant.Ghost   => "text-ink bg-white hover:bg-line-subtle border border-cyan-600",
        _                     => "text-white bg-brand-950 hover:bg-brand-900 shadow-sm",
    };
}

public enum ButtonVariant
{
    Primary,
    Outline,
    Ghost,
}

public enum ButtonSize
{
    Md,
    Lg,
}
