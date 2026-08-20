namespace RSD.Web.Data.Entities;

/// <summary>
/// A question/answer pair surfaced in public FAQ sections (and mirrored into FAQPage
/// JSON-LD). OwnerSlug optionally scopes an item to one detail page; empty = sitewide.
/// </summary>
public sealed record class FaqItem : ContentEntity, IHasDisplayOrder
{
    public required string Question { get; set; }
    public string AnswerHtml { get; set; } = "";
    public string OwnerSlug { get; set; } = "";
    public string Category { get; set; } = "";
    public bool ShowOnHome { get; set; }
    public int DisplayOrder { get; set; }
}
