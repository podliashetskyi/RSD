using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RSD.Web.Data.Entities;

namespace RSD.Web.Data.Configurations;

public sealed class TeamMemberConfiguration : IEntityTypeConfiguration<TeamMember>
{
    public void Configure(EntityTypeBuilder<TeamMember> b)
    {
        ContentEntityConfiguration.Apply(b, "team_members");
        b.Property(x => x.Name).HasMaxLength(200).IsRequired();
        b.Property(x => x.Role).HasMaxLength(200);
        b.Property(x => x.AvatarPath).HasMaxLength(500);
        b.HasIndex(x => x.DisplayOrder);
        b.HasIndex(x => x.IsManagement);
    }
}
