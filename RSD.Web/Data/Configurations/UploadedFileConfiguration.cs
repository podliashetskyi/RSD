using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RSD.Web.Data.Entities;

namespace RSD.Web.Data.Configurations;

public sealed class UploadedFileConfiguration : IEntityTypeConfiguration<UploadedFile>
{
    private static readonly ValueComparer<List<ImageVariant>> VariantsComparer = new(
        (a, b) => ReferenceEquals(a, b) || (a != null && b != null && a.SequenceEqual(b)),
        v => v.Aggregate(0, (acc, x) => HashCode.Combine(acc, x.GetHashCode())),
        v => v.ToList());

    public void Configure(EntityTypeBuilder<UploadedFile> b)
    {
        b.ToTable("uploaded_files");
        b.HasKey(x => x.Id);
        b.Property(x => x.Path).HasMaxLength(500).IsRequired();
        b.Property(x => x.OriginalName).HasMaxLength(500).IsRequired();
        b.Property(x => x.ContentType).HasMaxLength(100).IsRequired();
        b.Property(x => x.UploadedByUserId).HasMaxLength(450);
        b.Property(x => x.Variants)
            .HasColumnType("jsonb")
            .HasConversion(
                v => JsonSerializer.Serialize(v, JsonSerializerOptions.Default),
                v => JsonSerializer.Deserialize<List<ImageVariant>>(v, JsonSerializerOptions.Default) ?? new())
            .Metadata.SetValueComparer(VariantsComparer);
        b.HasIndex(x => x.Path).IsUnique();
        b.HasIndex(x => x.UploadedAt).IsDescending();
    }
}
