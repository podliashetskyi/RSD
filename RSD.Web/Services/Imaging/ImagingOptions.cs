namespace RSD.Web.Services.Imaging;

public sealed record class ImagingOptions
{
    public const string SectionName = "Imaging";

    public int WebPQuality { get; set; } = 82;
    public ImagingVariantsOptions Variants { get; set; } = new();
}

public sealed record class ImagingVariantsOptions
{
    public int Small { get; set; } = 480;
    public int Medium { get; set; } = 1024;
    public int Large { get; set; } = 1920;
}
