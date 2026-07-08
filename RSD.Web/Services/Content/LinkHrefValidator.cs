using RSD.Web.Services.Common;

namespace RSD.Web.Services.Content;

public static class LinkHrefValidator
{
    public const string SocialHrefMessage = "Social links must be empty or start with https://.";
    public const string MessengerHrefMessage = "Messenger links must be empty or start with https://, tg://, viber://, or whatsapp://.";
    public const string ContactHrefMessage = "Contact links must be empty or start with https://, mailto:, or tel:.";

    private static readonly HashSet<string> SocialSchemes = new(StringComparer.OrdinalIgnoreCase)
    {
        Uri.UriSchemeHttps,
    };

    private static readonly HashSet<string> MessengerSchemes = new(StringComparer.OrdinalIgnoreCase)
    {
        Uri.UriSchemeHttps,
        "tg",
        "viber",
        "whatsapp",
    };

    private static readonly HashSet<string> ContactSchemes = new(StringComparer.OrdinalIgnoreCase)
    {
        Uri.UriSchemeHttps,
        Uri.UriSchemeMailto,
        "tel",
    };

    public static bool IsValidSocialHref(string href) => IsValidOptionalAbsoluteHref(href, SocialSchemes);

    public static bool IsValidMessengerHref(string href) => IsValidOptionalAbsoluteHref(href, MessengerSchemes);

    public static bool IsValidContactHref(string href) => IsValidOptionalAbsoluteHref(href, ContactSchemes);

    public static Result<Unit> ValidateSocialHref(string href) => IsValidSocialHref(href)
        ? Result.Ok()
        : Result.Fail(SocialHrefMessage);

    public static Result<Unit> ValidateMessengerHref(string href) => IsValidMessengerHref(href)
        ? Result.Ok()
        : Result.Fail(MessengerHrefMessage);

    public static Result<Unit> ValidateContactHref(string href) => IsValidContactHref(href)
        ? Result.Ok()
        : Result.Fail(ContactHrefMessage);

    private static bool IsValidOptionalAbsoluteHref(string href, IReadOnlySet<string> allowedSchemes)
    {
        if (string.IsNullOrWhiteSpace(href)) return true;
        if (!string.Equals(href, href.Trim(), StringComparison.Ordinal)) return false;
        return Uri.TryCreate(href, UriKind.Absolute, out var uri)
               && allowedSchemes.Contains(uri.Scheme);
    }
}
