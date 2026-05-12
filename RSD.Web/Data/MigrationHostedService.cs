using Microsoft.EntityFrameworkCore;

namespace RSD.Web.Data;

public sealed class MigrationHostedService(
    IServiceProvider Services,
    IConfiguration Config,
    ILogger<MigrationHostedService> Log) : IHostedService
{
    public async Task StartAsync(CancellationToken ct)
    {
        if (!Config.GetValue("Database:AutoMigrate", true))
        {
            Log.LogInformation("Database:AutoMigrate is false; skipping startup migration.");
            return;
        }

        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        Log.LogInformation("Applying pending database migrations.");
        await db.Database.MigrateAsync(ct);
        Log.LogInformation("Database migrations applied.");
    }

    public Task StopAsync(CancellationToken ct) => Task.CompletedTask;
}
