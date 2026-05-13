using FluentAssertions;
using RSD.Web.Services.Slugs;

namespace RSD.Web.Tests.Unit.Slugs;

public sealed class SluggerSlugifyTests
{
    // Slugify itself doesn't touch the DB; we can call it via a null-context Slugger.
    private static readonly Slugger Sut = new(default!);

    [Theory]
    [InlineData("Hello World", "hello-world")]
    [InlineData("  Trim me  ", "trim-me")]
    [InlineData("Already-A-Slug", "already-a-slug")]
    [InlineData("multiple---dashes", "multiple-dashes")]
    [InlineData("Punctuation! ?, ; :", "punctuation")]
    public void Slugify_PlainAscii_NormalizesAsExpected(string input, string expected)
    {
        Sut.Slugify(input).Should().Be(expected);
    }

    [Theory]
    [InlineData("Привет мир", "privet-mir")]
    [InlineData("Тест", "test")]
    [InlineData("Щит — оружие", "shchit-oruzhie")]
    [InlineData("Київ", "kiyiv")]  // Russian-style и->i; canonical Ukrainian 'kyiv' would need language detection
    [InlineData("Україна", "ukrayina")]
    public void Slugify_Cyrillic_Transliterates(string input, string expected)
    {
        Sut.Slugify(input).Should().Be(expected);
    }

    [Theory]
    [InlineData("Café", "cafe")]
    [InlineData("Naïve", "naive")]
    [InlineData("Über", "uber")]
    public void Slugify_AccentedLatin_StripsDiacritics(string input, string expected)
    {
        Sut.Slugify(input).Should().Be(expected);
    }

    [Theory]
    [InlineData("")]
    [InlineData("???")]
    [InlineData("   ")]
    public void Slugify_AllNonWordInput_ReturnsEmptyString(string input)
    {
        Sut.Slugify(input).Should().BeEmpty();
    }
}
