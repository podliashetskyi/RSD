namespace RSD.Web.Data.Seed;

public interface ISeeder
{
    Task SeedAsync(CancellationToken ct);
}
