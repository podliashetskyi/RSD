namespace RSD.Web.Services.Seo;

/// <summary>Canonical site origin (scheme + host), normalized without a trailing slash.</summary>
public sealed record Origin
{
    public string Value { get; }

    public Origin(string value) => Value = value.TrimEnd('/');

    public bool IsEmpty => Value.Length == 0;

    public override string ToString() => Value;
}

/// <summary>Resolves the effective origin for SEO output: configured BaseUrl wins, request host is the fallback.</summary>
public static class RequestOrigin
{
    public static string Resolve(SeoOptions options, HttpRequest request)
    {
        var configured = new Origin(options.BaseUrl);
        return configured.IsEmpty ? $"{request.Scheme}://{request.Host}" : configured.Value;
    }
}
