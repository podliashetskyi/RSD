using RSD.Web.Data.Entities;

namespace RSD.Web.Components.Admin.Shared.BodyForms;

public sealed record class TwoColForm
{
    public bool Enabled { get; set; }
    public string Left { get; set; } = "";
    public string Right { get; set; } = "";

    public static TwoColForm From(TwoColumnText? t) => t is null
        ? new TwoColForm()
        : new TwoColForm { Enabled = true, Left = t.Left, Right = t.Right };

    public TwoColumnText? ToEntity() => Enabled ? new TwoColumnText(Left, Right) : null;
}
