namespace RSD.Web.Services.Common;

/// <summary>
/// Sanitizes HTML coming from rich-text editors before it is persisted.
/// </summary>
public interface IContentHtmlSanitizer
{
    string Sanitize(string html);
}
