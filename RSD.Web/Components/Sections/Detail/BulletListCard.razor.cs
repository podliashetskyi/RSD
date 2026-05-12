#pragma warning disable S1144, S4487, S2933
using Microsoft.AspNetCore.Components;

namespace RSD.Web.Components.Sections.Detail;

public partial class BulletListCard
{
    [Parameter, EditorRequired] public string Heading { get; set; } = "";
    [Parameter, EditorRequired] public IReadOnlyList<string> Items { get; set; } = [];
    [Parameter] public BulletListHeadingSize HeadingSize { get; set; } = BulletListHeadingSize.Small;
    [Parameter] public bool WithShadow { get; set; }

    private string HeadingSizeClass => HeadingSize == BulletListHeadingSize.Large
        ? "text-[28px] lg:text-[36px]"
        : "text-[24px]";

    private string HeadingLeadingClass => HeadingSize == BulletListHeadingSize.Large
        ? "leading-9"
        : "leading-8";

    private string ShadowClass => WithShadow ? "shadow-sm" : "";
}

public enum BulletListHeadingSize { Small, Large }
