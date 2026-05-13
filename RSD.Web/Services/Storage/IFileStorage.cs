namespace RSD.Web.Services.Storage;

public interface IFileStorage
{
    Task<StoredFile> SaveAsync(string subfolder, Stream content, string suggestedFileName, string contentType, CancellationToken ct);
    Task DeleteAsync(string path, CancellationToken ct);
    Task<Stream> OpenReadAsync(string path, CancellationToken ct);
    string GetPublicUrl(string path);
}
