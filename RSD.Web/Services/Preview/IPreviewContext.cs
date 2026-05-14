namespace RSD.Web.Services.Preview;

/// <summary>
/// Request-scoped flag: when true, content services include drafts in detail lookups.
/// Set by the preview route handler after the HMAC token has verified.
/// </summary>
public interface IPreviewContext
{
    bool IsPreview { get; set; }
}
