using RSD.Web.Data.Entities;

namespace RSD.Web.Services.Imaging;

/// <summary>
/// The one upload pipeline: process (variants, sanitization) and persist the UploadedFile
/// row. Used by the admin ImageUploader component and the MCP upload_image tool alike.
/// </summary>
public interface IImageUploadService
{
    Task<UploadedFile> UploadAsync(
        string subfolder, Stream content, string fileName, string contentType, string uploadedByUserId, CancellationToken ct);
}
