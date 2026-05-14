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
        b.Property(x => x.Platform).HasConversion<string>().HasMaxLength(40).IsRequired();
        b.Property(x => x.Domain).HasConversion<string>().HasMaxLength(40).IsRequired();
        b.Property(x => x.Complexity).HasConversion<string>().HasMaxLength(40).IsRequired();
        b.Property(x => x.Timeline).HasConversion<string>().HasMaxLength(40).IsRequired();
        b.Property(x => x.EstimateMin).HasColumnType("numeric(12,2)");
        b.Property(x => x.EstimateMax).HasColumnType("numeric(12,2)");
        b.Property(x => x.ContactName).HasMaxLength(200).IsRequired();
        b.Property(x => x.ContactEmail).HasMaxLength(320).IsRequired();
        b.Property(x => x.Company).HasMaxLength(200).IsRequired();
        b.Property(x => x.ProjectDescription).HasMaxLength(8000).IsRequired();
        b.Property(x => x.HandledByUserId).HasMaxLength(450);
        b.HasIndex(x => x.SubmittedAt).IsDescending();
        b.HasIndex(x => x.IsHandled);
    }
}
