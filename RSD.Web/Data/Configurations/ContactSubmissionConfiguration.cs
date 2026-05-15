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
        b.Property(x => x.Name).HasMaxLength(FieldLimits.ContactSubmission.Name).IsRequired();
        b.Property(x => x.Email).HasMaxLength(FieldLimits.ContactSubmission.Email).IsRequired();
        b.Property(x => x.Subject).HasMaxLength(FieldLimits.ContactSubmission.Subject).IsRequired();
        b.Property(x => x.Message).HasMaxLength(FieldLimits.ContactSubmission.Message).IsRequired();
        b.Property(x => x.HandledByUserId).HasMaxLength(FieldLimits.UploadedFile.UploadedByUserId);
        b.HasIndex(x => x.SubmittedAt).IsDescending();
        b.HasIndex(x => x.IsHandled);
    }
}
