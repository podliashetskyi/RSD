using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using RSD.Web.Services.Common;

namespace RSD.Web.Services.Preview;

public sealed class HmacPreviewTokenSigner(IOptions<PreviewOptions> Options) : IPreviewTokenSigner
{
    private const char Separator = '.';

    public string Sign(PreviewClaims claims)
    {
        var payload = SerializeClaims(claims);
        var signature = ComputeSignature(payload);
        return $"{ToBase64Url(payload)}{Separator}{ToBase64Url(signature)}";
    }

    public Result<PreviewClaims> Verify(string token)
    {
        var parts = token.Split(Separator);
        if (parts.Length != 2) return Result<PreviewClaims>.Failure("malformed token");
        return VerifyParts(parts[0], parts[1]);
    }

    private Result<PreviewClaims> VerifyParts(string payloadEncoded, string signatureEncoded)
    {
        var payload = TryDecode(payloadEncoded);
        var signature = TryDecode(signatureEncoded);
        if (payload is null || signature is null) return Result<PreviewClaims>.Failure("malformed token");
        if (!IsValidSignature(payload, signature)) return Result<PreviewClaims>.Failure("invalid signature");
        return DeserializeAndValidate(payload);
    }

    private bool IsValidSignature(byte[] payload, byte[] signature)
    {
        var expected = ComputeSignature(payload);
        return CryptographicOperations.FixedTimeEquals(expected, signature);
    }

    private byte[] ComputeSignature(byte[] payload)
    {
        var key = Encoding.UTF8.GetBytes(Options.Value.SigningKey);
        using var hmac = new HMACSHA256(key);
        return hmac.ComputeHash(payload);
    }

    private static byte[] SerializeClaims(PreviewClaims claims) =>
        JsonSerializer.SerializeToUtf8Bytes(claims, JsonSerializerOptions.Default);

    private static Result<PreviewClaims> DeserializeAndValidate(byte[] payload)
    {
        var claims = JsonSerializer.Deserialize<PreviewClaims>(payload, JsonSerializerOptions.Default);
        if (claims is null) return Result<PreviewClaims>.Failure("malformed token");
        if (claims.ExpiresAt < DateTimeOffset.UtcNow) return Result<PreviewClaims>.Failure("token expired");
        return Result<PreviewClaims>.Success(claims);
    }

    private static byte[]? TryDecode(string encoded)
    {
        try { return FromBase64Url(encoded); }
        catch (FormatException) { return null; }
    }

    private static string ToBase64Url(byte[] bytes) =>
        Convert.ToBase64String(bytes).TrimEnd('=').Replace('+', '-').Replace('/', '_');

    private static byte[] FromBase64Url(string encoded)
    {
        var padded = encoded.Replace('-', '+').Replace('_', '/');
        var padding = (4 - padded.Length % 4) % 4;
        return Convert.FromBase64String(padded + new string('=', padding));
    }
}
