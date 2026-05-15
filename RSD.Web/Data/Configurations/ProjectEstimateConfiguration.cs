using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RSD.Web.Data.Entities;

namespace RSD.Web.Data.Configurations;

public sealed class ProjectEstimateConfiguration : IEntityTypeConfiguration<ProjectEstimate>
{
    public void Configure(EntityTypeBuilder<ProjectEstimate> b)
    {
        b.ToTable("project_estimates");
        b.HasKey(x => x.Id);
        b.Property(x => x.Platform).HasConversion<string>().HasMaxLength(FieldLimits.ProjectEstimate.EnumLabel).IsRequired();
        b.Property(x => x.Domain).HasConversion<string>().HasMaxLength(FieldLimits.ProjectEstimate.EnumLabel).IsRequired();
        b.Property(x => x.Complexity).HasConversion<string>().HasMaxLength(FieldLimits.ProjectEstimate.EnumLabel).IsRequired();
        b.Property(x => x.Timeline).HasConversion<string>().HasMaxLength(FieldLimits.ProjectEstimate.EnumLabel).IsRequired();
        b.Property(x => x.EstimateMin).HasColumnType("numeric(12,2)");
        b.Property(x => x.EstimateMax).HasColumnType("numeric(12,2)");
        b.Property(x => x.ContactName).HasMaxLength(FieldLimits.ProjectEstimate.ContactName).IsRequired();
        b.Property(x => x.ContactEmail).HasMaxLength(FieldLimits.ProjectEstimate.ContactEmail).IsRequired();
        b.Property(x => x.Company).HasMaxLength(FieldLimits.ProjectEstimate.Company).IsRequired();
        b.Property(x => x.ProjectDescription).HasMaxLength(FieldLimits.ProjectEstimate.ProjectDescription).IsRequired();
        b.Property(x => x.HandledByUserId).HasMaxLength(FieldLimits.UploadedFile.UploadedByUserId);
        b.HasIndex(x => x.SubmittedAt).IsDescending();
        b.HasIndex(x => x.IsHandled);
    }
}
