using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RSD.Web.Data.Entities;

namespace RSD.Web.Data.Configurations;

public sealed class ContactSubmissionConfiguration : IEntityTypeConfiguration<ContactSubmission>
{
    public void Configure(EntityTypeBuilder<ContactSubmission> b)
    {
        b.ToTable("contact_submissions");
        b.HasKey(x => x.Id);
        b.Property(x => x.Name).HasMaxLength(200).IsRequired();
        b.Property(x => x.Email).HasMaxLength(320).IsRequired();
        b.Property(x => x.Subject).HasMaxLength(500).IsRequired();
        b.Property(x => x.Message).HasMaxLength(8000).IsRequired();
        b.Property(x => x.HandledByUserId).HasMaxLength(450);
        b.HasIndex(x => x.SubmittedAt).IsDescending();
        b.HasIndex(x => x.IsHandled);
    }
}
