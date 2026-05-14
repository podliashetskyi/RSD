namespace RSD.Web.Services.Storage;

/// <summary>
/// Maintains UploadedFile.RefCount as content entities are saved.
/// Callers pass the old and new sets of file paths for an entity; the tracker
/// applies the delta (+1 for newly referenced files, -1 for newly unreferenced files).
/// </summary>
public interface IFileRefCountTracker
{
    Task ApplyDeltaAsync(IEnumerable<string> oldPaths, IEnumerable<string> newPaths, CancellationToken ct);
}
