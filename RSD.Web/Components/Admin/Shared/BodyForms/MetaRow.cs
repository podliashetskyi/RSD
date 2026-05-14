using RSD.Web.Data.Entities;

namespace RSD.Web.Components.Admin.Shared.BodyForms;

public sealed record class MetaRow
{
    public string Label { get; set; } = "";
    public string Value { get; set; } = "";

    public static MetaRow From(MetaItem m) => new() { Label = m.Label, Value = m.Value };
    public MetaItem ToEntity() => new(Label, Value);
}
