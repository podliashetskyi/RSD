namespace RSD.Web.Services.Preview;

public record PreviewClaims(string EntityType, string Slug, DateTimeOffset ExpiresAt);
