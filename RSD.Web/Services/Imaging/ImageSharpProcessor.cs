using System.Text;
using Microsoft.Extensions.Options;
using RSD.Web.Data.Entities;
using RSD.Web.Services.Storage;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Webp;
using SixLabors.ImageSharp.Metadata;
using SixLabors.ImageSharp.Processing;

namespace RSD.Web.Services.Imaging;

public sealed class ImageSharpProcessor(
    IFileStorage Storage,
    SvgSanitizer Svg,
    IOptions<ImagingOptions> Options) : IImageProcessor
{
    private const string SvgContentType = "image/svg+xml";

    public async Task<ProcessedUpload> ProcessAsync(string subfolder, Stream original, string originalFileName, string contentType, CancellationToken ct)
    {
        if (IsSvg(contentType)) return await ProcessSvgAsync(subfolder, original, originalFileName, ct);
        return await ProcessRasterAsync(subfolder, original, originalFileName, contentType, ct);
    }

    private async Task<ProcessedUpload> ProcessSvgAsync(string subfolder, Stream original, string originalFileName, CancellationToken ct)
    {
        using var reader = new StreamReader(original, Encoding.UTF8, leaveOpen: true);
        var raw = await reader.ReadToEndAsync(ct);
        var sanitized = Svg.Sanitize(raw);
        var bytes = Encoding.UTF8.GetBytes(sanitized);
        await using var ms = new MemoryStream(bytes);
        var stored = await Storage.SaveAsync(subfolder, ms, originalFileName, SvgContentType, ct);
        return new ProcessedUpload(stored, []);
    }

    private async Task<ProcessedUpload> ProcessRasterAsync(string subfolder, Stream original, string originalFileName, string contentType, CancellationToken ct)
    {
        using var image = await Image.LoadAsync(original, ct);
        var originalStored = await StoreOriginalAsync(subfolder, image, originalFileName, contentType, ct);
        var variants = await EmitVariantsAsync(subfolder, image, originalFileName, ct);
        return new ProcessedUpload(originalStored, variants);
    }

    private async Task<StoredFile> StoreOriginalAsync(string subfolder, Image image, string originalFileName, string contentType, CancellationToken ct)
    {
        await using var ms = new MemoryStream();
        await image.SaveAsync(ms, image.Metadata.DecodedImageFormat ?? WebpFormat.Instance, ct);
        ms.Position = 0;
        return await Storage.SaveAsync(subfolder, ms, OriginalFileName(originalFileName), contentType, ct);
    }

    private async Task<IReadOnlyList<ImageVariant>> EmitVariantsAsync(string subfolder, Image image, string originalFileName, CancellationToken ct)
    {
        var sizes = Options.Value.Variants;
        var quality = Options.Value.WebPQuality;
        var encoder = new WebpEncoder { Quality = quality };
        var emitted = new List<ImageVariant>(3);
        foreach (var (size, width) in EnumerateSizes(sizes))
        {
            emitted.Add(await EmitOneVariantAsync(subfolder, image, originalFileName, size, width, encoder, ct));
        }
        return emitted;
    }

    private async Task<ImageVariant> EmitOneVariantAsync(string subfolder, Image image, string originalFileName, string size, int maxWidth, WebpEncoder encoder, CancellationToken ct)
    {
        var (width, height) = ResolveDimensions(image.Width, image.Height, maxWidth);
        using var clone = image.Clone(ctx => ctx.Resize(width, height));
        await using var ms = new MemoryStream();
        await clone.SaveAsync(ms, encoder, ct);
        ms.Position = 0;
        var name = VariantFileName(originalFileName, size);
        var stored = await Storage.SaveAsync(subfolder, ms, name, "image/webp", ct);
        return new ImageVariant(size, stored.Path, width, height, stored.Bytes);
    }

    private static (int Width, int Height) ResolveDimensions(int originalWidth, int originalHeight, int maxWidth)
    {
        if (originalWidth <= maxWidth) return (originalWidth, originalHeight);
        var ratio = (double)maxWidth / originalWidth;
        return (maxWidth, (int)Math.Round(originalHeight * ratio));
    }

    private static IEnumerable<(string Size, int Width)> EnumerateSizes(ImagingVariantsOptions v) =>
    [
        ("small", v.Small),
        ("medium", v.Medium),
        ("large", v.Large),
    ];

    private static bool IsSvg(string contentType) =>
        string.Equals(contentType, SvgContentType, StringComparison.OrdinalIgnoreCase);

    private static string OriginalFileName(string name)
    {
        var stem = Path.GetFileNameWithoutExtension(name);
        var ext = Path.GetExtension(name);
        if (stem.Length == 0) stem = "image";
        return $"{stem}-original{ext}";
    }

    private static string VariantFileName(string name, string size)
    {
        var stem = Path.GetFileNameWithoutExtension(name);
        if (stem.Length == 0) stem = "image";
        return $"{stem}-{size}.webp";
    }
}
