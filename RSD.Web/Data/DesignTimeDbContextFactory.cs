using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace RSD.Web.Data;

/// <summary>
/// Used only by the EF Core CLI (`dotnet ef migrations add ...`). The runtime
/// container builds the DbContext via <see cref="IDbContextFactory{AppDbContext}"/>
/// with the audit interceptor wired up; design time does not need the interceptor.
/// </summary>
public sealed class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql("Host=localhost;Database=rsd_design;Username=design;Password=design")
            .Options;
        return new AppDbContext(options);
    }
}
