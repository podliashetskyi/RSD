using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using RSD.Web.Data;
using RSD.Web.Data.Entities;

namespace RSD.Web.Services.Slugs;

public sealed partial class Slugger(AppDbContext Db) : ISlugger
{
    private const int MaxAttempts = 1000;

    public string Slugify(string source) => SlugifyCore(source);

    public async Task<bool> IsAvailableAsync<TEntity>(string slug, Guid? currentId, CancellationToken ct)
        where TEntity : ContentEntity
    {
        var collision = await Db.Set<TEntity>()
            .AsNoTracking()
            .Where(e => e.Slug == slug && !e.IsDeleted)
            .Where(e => currentId == null || e.Id != currentId)
            .AnyAsync(ct);
        return !collision;
    }

    public async Task<string> GenerateUniqueAsync<TEntity>(string source, Guid? currentId, CancellationToken ct)
        where TEntity : ContentEntity
    {
        var baseSlug = SlugifyCore(source);
        if (baseSlug.Length == 0) baseSlug = "untitled";
        return await FindNextAvailable<TEntity>(baseSlug, currentId, ct);
    }

    private async Task<string> FindNextAvailable<TEntity>(string baseSlug, Guid? currentId, CancellationToken ct)
        where TEntity : ContentEntity
    {
        for (var n = 1; n <= MaxAttempts; n++)
        {
            var candidate = n == 1 ? baseSlug : $"{baseSlug}-{n}";
            if (await IsAvailableAsync<TEntity>(candidate, currentId, ct)) return candidate;
        }
        throw new InvalidOperationException($"Unable to find a unique slug derived from '{baseSlug}' after {MaxAttempts} attempts.");
    }

    private static string SlugifyCore(string source)
    {
        var transliterated = Transliterate(source);
        var normalized = StripDiacritics(transliterated);
        var lowered = normalized.ToLowerInvariant();
        var dashed = NonWordPattern().Replace(lowered, "-");
        return dashed.Trim('-');
    }

    private static string Transliterate(string source)
    {
        var sb = new StringBuilder(source.Length);
        foreach (var ch in source) sb.Append(CyrillicMap.TryGetValue(ch, out var replacement) ? replacement : ch.ToString());
        return sb.ToString();
    }

    private static string StripDiacritics(string source)
    {
        var decomposed = source.Normalize(NormalizationForm.FormD);
        var sb = new StringBuilder(decomposed.Length);
        foreach (var ch in decomposed)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(ch) != UnicodeCategory.NonSpacingMark) sb.Append(ch);
        }
        return sb.ToString().Normalize(NormalizationForm.FormC);
    }

    [GeneratedRegex("[^a-z0-9]+", RegexOptions.CultureInvariant)]
    private static partial Regex NonWordPattern();

    private static readonly Dictionary<char, string> CyrillicMap = new()
    {
        ['а'] = "a", ['б'] = "b", ['в'] = "v", ['г'] = "g", ['д'] = "d", ['е'] = "e",
        ['ё'] = "yo", ['ж'] = "zh", ['з'] = "z", ['и'] = "i", ['й'] = "y", ['к'] = "k",
        ['л'] = "l", ['м'] = "m", ['н'] = "n", ['о'] = "o", ['п'] = "p", ['р'] = "r",
        ['с'] = "s", ['т'] = "t", ['у'] = "u", ['ф'] = "f", ['х'] = "kh", ['ц'] = "ts",
        ['ч'] = "ch", ['ш'] = "sh", ['щ'] = "shch", ['ъ'] = "", ['ы'] = "y", ['ь'] = "",
        ['э'] = "e", ['ю'] = "yu", ['я'] = "ya",
        ['А'] = "A", ['Б'] = "B", ['В'] = "V", ['Г'] = "G", ['Д'] = "D", ['Е'] = "E",
        ['Ё'] = "Yo", ['Ж'] = "Zh", ['З'] = "Z", ['И'] = "I", ['Й'] = "Y", ['К'] = "K",
        ['Л'] = "L", ['М'] = "M", ['Н'] = "N", ['О'] = "O", ['П'] = "P", ['Р'] = "R",
        ['С'] = "S", ['Т'] = "T", ['У'] = "U", ['Ф'] = "F", ['Х'] = "Kh", ['Ц'] = "Ts",
        ['Ч'] = "Ch", ['Ш'] = "Sh", ['Щ'] = "Shch", ['Ъ'] = "", ['Ы'] = "Y", ['Ь'] = "",
        ['Э'] = "E", ['Ю'] = "Yu", ['Я'] = "Ya",
        // Ukrainian extras
        ['ї'] = "yi", ['Ї'] = "Yi", ['є'] = "ye", ['Є'] = "Ye",
        ['і'] = "i", ['І'] = "I", ['ґ'] = "g", ['Ґ'] = "G",
    };
}
