using RSD.Web.Data.Entities;
using RSD.Web.Services.Storage;

namespace RSD.Web.Services.Imaging;

public record ProcessedUpload(StoredFile OriginalFile, IReadOnlyList<ImageVariant> Variants);
