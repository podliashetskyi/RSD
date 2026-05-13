using System.Text.RegularExpressions;

namespace RSD.Web.Services.Storage;

public sealed partial class LocalDiskFileStorage(IWebHostEnvironment Env) : IFileStorage
{
    private const string UploadsSegment = "uploads";

    public async Task<StoredFile> SaveAsync(string subfolder, Stream content, string suggestedFileName, string contentType, CancellationToken ct)
    {
        var (relativePath, absolutePath) = BuildPaths(subfolder, suggestedFileName);
        Directory.CreateDirectory(Path.GetDirectoryName(absolutePath)!);
        var bytes = await CopyToDiskAsync(content, absolutePath, ct);
        return new StoredFile(ToForwardSlash(relativePath), bytes, contentType);
    }

    public Task DeleteAsync(string path, CancellationToken ct)
    {
        var absolute = ResolveAbsolute(path);
        if (File.Exists(absolute)) File.Delete(absolute);
        return Task.CompletedTask;
    }

    public Task<Stream> OpenReadAsync(string path, CancellationToken ct)
    {
        var absolute = ResolveAbsolute(path);
        Stream stream = new FileStream(absolute, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, useAsync: true);
        return Task.FromResult(stream);
    }

    public string GetPublicUrl(string path) => "/" + ToForwardSlash(path).TrimStart('/');

    private (string Relative, string Absolute) BuildPaths(string subfolder, string suggestedFileName)
    {
        var safeSubfolder = SanitizeSegment(subfolder);
        var safeName = MakeUniqueFileName(suggestedFileName);
        var now = DateTime.UtcNow;
        var relative = Path.Combine(UploadsSegment, safeSubfolder, now.Year.ToString("0000"), now.Month.ToString("00"), safeName);
        var absolute = Path.Combine(Env.WebRootPath, relative);
        return (relative, absolute);
    }

    private string ResolveAbsolute(string path) =>
        Path.Combine(Env.WebRootPath, ToOsSlash(path).TrimStart(Path.DirectorySeparatorChar));

    private static async Task<long> CopyToDiskAsync(Stream content, string absolutePath, CancellationToken ct)
    {
        await using var output = new FileStream(absolutePath, FileMode.Create, FileAccess.Write, FileShare.None, 81920, useAsync: true);
        await content.CopyToAsync(output, ct);
        return output.Length;
    }

    private static string MakeUniqueFileName(string suggested)
    {
        var sanitized = SanitizeFileName(suggested);
        var extension = Path.GetExtension(sanitized);
        var stem = Path.GetFileNameWithoutExtension(sanitized);
        if (stem.Length == 0) stem = "file";
        return $"{Guid.NewGuid():N}-{stem}{extension}";
    }

    private static string SanitizeSegment(string segment) => InvalidPathPattern().Replace(segment, "-").Trim('-', '.', '/');

    private static string SanitizeFileName(string name) => InvalidFileNamePattern().Replace(Path.GetFileName(name), "-");

    private static string ToForwardSlash(string p) => p.Replace(Path.DirectorySeparatorChar, '/');

    private static string ToOsSlash(string p) => p.Replace('/', Path.DirectorySeparatorChar);

    [GeneratedRegex("[^a-zA-Z0-9_-]+", RegexOptions.CultureInvariant)]
    private static partial Regex InvalidPathPattern();

    [GeneratedRegex("[^a-zA-Z0-9._-]+", RegexOptions.CultureInvariant)]
    private static partial Regex InvalidFileNamePattern();
}
