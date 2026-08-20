using Microsoft.EntityFrameworkCore;
using RSD.Web.Data;
using RSD.Web.Data.Entities;

namespace RSD.Web.Services.Imaging;

public sealed class ImageUploadService(IImageProcessor Processor, IDbContextFactory<AppDbContext> DbFactory) : IImageUploadService
{
    public async Task<UploadedFile> UploadAsync(
        string subfolder, Stream content, string fileName, string contentType, string uploadedByUserId, CancellationToken ct)
    {
        var processed = await Processor.ProcessAsync(subfolder, content, fileName, contentType, ct);
        var entity = new UploadedFile
        {
            Path = processed.OriginalFile.Path,
            OriginalName = fileName,
            ContentType = contentType,
            Bytes = processed.OriginalFile.Bytes,
            UploadedByUserId = uploadedByUserId,
            Variants = processed.Variants.ToList(),
        };
        await using var db = await DbFactory.CreateDbContextAsync(ct);
        db.UploadedFiles.Add(entity);
        await db.SaveChangesAsync(ct);
        return entity;
    }
}
