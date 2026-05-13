using Testcontainers.PostgreSql;

namespace RSD.Web.Tests.Integration.Fixtures;

public sealed class PostgresFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer Container = new PostgreSqlBuilder("postgres:16-alpine")
        .WithDatabase("rsd_test")
        .WithUsername("rsd_test")
        .WithPassword("rsd_test")
        .Build();

    public string ConnectionString => Container.GetConnectionString();

    public Task InitializeAsync() => Container.StartAsync();

    public Task DisposeAsync() => Container.DisposeAsync().AsTask();
}

[CollectionDefinition(nameof(PostgresCollection))]
public sealed class PostgresCollection : ICollectionFixture<PostgresFixture> { }
