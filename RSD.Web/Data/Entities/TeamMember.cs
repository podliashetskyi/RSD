namespace RSD.Web.Data.Entities;

public sealed record class TeamMember : ContentEntity, IHasDisplayOrder
{
    public required string Name { get; set; }
    public string Role { get; set; } = "";
    public string AvatarPath { get; set; } = "";
    public int DisplayOrder { get; set; }
    public bool IsManagement { get; set; }
}
