using RSD.Web.Data.Entities;

namespace RSD.Web.Components.Admin.Shared.BodyForms;

public sealed record class BadgeRow
{
    public string Text { get; set; } = "";
    public string BgClass { get; set; } = "";
    public string TextClass { get; set; } = "";

    public static BadgeRow From(BadgePill b) => new() { Text = b.Text, BgClass = b.BgClass, TextClass = b.TextClass };
    public BadgePill ToEntity() => new(Text, BgClass, TextClass);
}
