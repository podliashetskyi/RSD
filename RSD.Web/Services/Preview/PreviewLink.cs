using Microsoft.Extensions.Options;

namespace RSD.Web.Services.Preview;

/// <summary>
/// URL helpers around <see cref="IPreviewTokenSigner"/> so that callers don't
/// repeat slug → entity-type → URL construction logic.
/// </summary>
public sealed class PreviewLink(IPreviewTokenSigner Signer, IOptions<PreviewOptions> Options)
{
    public string Build(string entityType, string slug)
    {
        var claims = new PreviewClaims(entityType, slug, DateTimeOffset.UtcNow.AddMinutes(Options.Value.TtlMinutes));
        var token = Signer.Sign(claims);
        return $"/preview/{entityType}/{Uri.EscapeDataString(slug)}?token={Uri.EscapeDataString(token)}";
    }

    public bool Verify(string entityType, string slug, string? token)
    {
        if (string.IsNullOrEmpty(token)) return false;
        var verified = Signer.Verify(token);
        if (!verified.Ok || verified.Value is null) return false;
        return string.Equals(verified.Value.EntityType, entityType, StringComparison.Ordinal)
            && string.Equals(verified.Value.Slug, slug, StringComparison.Ordinal);
    }
}
