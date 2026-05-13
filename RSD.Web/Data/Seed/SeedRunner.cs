namespace RSD.Web.Data.Seed;

public sealed class SeedRunner(IServiceScopeFactory ScopeFactory, ILogger<SeedRunner> Log) : IHostedService
{
    public async Task StartAsync(CancellationToken ct)
    {
        using var scope = ScopeFactory.CreateScope();
        var seeders = scope.ServiceProvider.GetServices<ISeeder>().ToList();
        Log.LogInformation("Seed runner: {Count} seeders registered.", seeders.Count);
        foreach (var seeder in seeders) await RunOneAsync(seeder, ct);
        Log.LogInformation("Seed runner: complete.");
    }

    public Task StopAsync(CancellationToken ct) => Task.CompletedTask;

    private async Task RunOneAsync(ISeeder seeder, CancellationToken ct)
    {
        try { await seeder.SeedAsync(ct); }
        catch (Exception ex) { Log.LogError(ex, "Seeder {Type} failed.", seeder.GetType().Name); }
    }
}
