namespace RSD.Web.Data.Entities;

public sealed record class MissionStat : ContentEntity
{
    public required string Label { get; set; }
    public string Number { get; set; } = "";
    public string Symbol { get; set; } = "";
    public int DisplayOrder { get; set; }
}
