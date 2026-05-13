namespace RSD.Web.Data.Entities;

public sealed record class Testimonial : ContentEntity, IHasDisplayOrder
{
    public required string Title { get; set; }
    public required string Quote { get; set; }
    public string AvatarPath { get; set; } = "";
    public required string AuthorName { get; set; }
    public string AuthorRole { get; set; } = "";
    public bool DisplayOnHome { get; set; } = true;
    public int DisplayOrder { get; set; }
}
