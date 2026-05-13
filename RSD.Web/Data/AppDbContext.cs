using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using RSD.Web.Data.Entities;
using RSD.Web.Services.Auth;

namespace RSD.Web.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : IdentityDbContext<AdminUser>(options)
{
    public DbSet<ContactSubmission> ContactSubmissions => Set<ContactSubmission>();
    public DbSet<AuditLogEntry> AuditLogEntries => Set<AuditLogEntry>();
    public DbSet<UploadedFile> UploadedFiles => Set<UploadedFile>();

    public DbSet<Testimonial> Testimonials => Set<Testimonial>();
    public DbSet<TeamMember> TeamMembers => Set<TeamMember>();
    public DbSet<Partner> Partners => Set<Partner>();
    public DbSet<Value> Values => Set<Value>();
    public DbSet<MissionStat> MissionStats => Set<MissionStat>();
    public DbSet<TechStackItem> TechStackItems => Set<TechStackItem>();
    public DbSet<ContactPoint> ContactPoints => Set<ContactPoint>();
    public DbSet<MessengerLink> MessengerLinks => Set<MessengerLink>();
    public DbSet<SocialLink> SocialLinks => Set<SocialLink>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}
