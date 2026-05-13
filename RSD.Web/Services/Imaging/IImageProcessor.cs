namespace RSD.Web.Services.Imaging;

public interface IImageProcessor
{
    Task<ProcessedUpload> ProcessAsync(string subfolder, Stream original, string originalFileName, string contentType, CancellationToken ct);
}
