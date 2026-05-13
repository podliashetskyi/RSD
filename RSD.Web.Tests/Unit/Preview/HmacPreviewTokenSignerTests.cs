using FluentAssertions;
using Microsoft.Extensions.Options;
using RSD.Web.Services.Preview;

namespace RSD.Web.Tests.Unit.Preview;

public sealed class HmacPreviewTokenSignerTests
{
    private static HmacPreviewTokenSigner BuildSigner(string key = "unit-test-key") =>
        new(Options.Create(new PreviewOptions { SigningKey = key, TtlMinutes = 60 }));

    [Fact]
    public void Sign_ThenVerify_RoundTripsClaims()
    {
        var signer = BuildSigner();
        var claims = new PreviewClaims("BlogPost", "hello-world", DateTimeOffset.UtcNow.AddHours(1));

        var token = signer.Sign(claims);
        var result = signer.Verify(token);

        result.Ok.Should().BeTrue();
        result.Value!.EntityType.Should().Be("BlogPost");
        result.Value!.Slug.Should().Be("hello-world");
    }

    [Fact]
    public void Verify_TamperedPayload_Fails()
    {
        var signer = BuildSigner();
        var token = signer.Sign(new PreviewClaims("BlogPost", "hello", DateTimeOffset.UtcNow.AddHours(1)));

        var parts = token.Split('.');
        var tamperedPayload = parts[0].Replace('a', 'b').Replace('A', 'B');
        var tampered = $"{tamperedPayload}.{parts[1]}";

        signer.Verify(tampered).Ok.Should().BeFalse();
    }

    [Fact]
    public void Verify_TamperedSignature_Fails()
    {
        var signer = BuildSigner();
        var token = signer.Sign(new PreviewClaims("BlogPost", "hello", DateTimeOffset.UtcNow.AddHours(1)));

        var parts = token.Split('.');
        var first = parts[1][0];
        var swapped = first == 'A' ? 'B' : 'A';
        var tampered = $"{parts[0]}.{swapped}{parts[1][1..]}";

        signer.Verify(tampered).Ok.Should().BeFalse();
    }

    [Fact]
    public void Verify_DifferentSigningKey_Fails()
    {
        var alice = BuildSigner("alice-key");
        var bob = BuildSigner("bob-key");
        var token = alice.Sign(new PreviewClaims("BlogPost", "hello", DateTimeOffset.UtcNow.AddHours(1)));

        bob.Verify(token).Ok.Should().BeFalse();
    }

    [Fact]
    public void Verify_ExpiredToken_Fails()
    {
        var signer = BuildSigner();
        var expired = signer.Sign(new PreviewClaims("BlogPost", "hello", DateTimeOffset.UtcNow.AddSeconds(-1)));

        var result = signer.Verify(expired);
        result.Ok.Should().BeFalse();
        result.Error.Should().Contain("expired");
    }

    [Fact]
    public void Verify_MalformedToken_Fails()
    {
        BuildSigner().Verify("not.a.valid.token").Ok.Should().BeFalse();
        BuildSigner().Verify("no-separator").Ok.Should().BeFalse();
    }
}
