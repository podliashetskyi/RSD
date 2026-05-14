using RSD.Web.Data.Entities;

namespace RSD.Web.Components.Admin.Shared.BodyForms;

public sealed record class TestimonialForm
{
    public bool Enabled { get; set; }
    public string Quote { get; set; } = "";
    public string AuthorName { get; set; } = "";
    public string AuthorRole { get; set; } = "";
    public string AvatarPath { get; set; } = "";

    public static TestimonialForm From(EmbeddedTestimonial? t) => t is null
        ? new TestimonialForm()
        : new TestimonialForm
        {
            Enabled = true,
            Quote = t.Quote,
            AuthorName = t.AuthorName,
            AuthorRole = t.AuthorRole,
            AvatarPath = t.AvatarPath
        };

    public EmbeddedTestimonial? ToEntity() => Enabled
        ? new EmbeddedTestimonial(Quote, AuthorName, AuthorRole, AvatarPath)
        : null;
}
