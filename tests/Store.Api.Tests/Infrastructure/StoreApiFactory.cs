using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Npgsql;
using Testcontainers.PostgreSql;

[assembly: AssemblyFixture(typeof(Store.Api.Tests.Infrastructure.StoreApiFactory))]

namespace Store.Api.Tests.Infrastructure;

/// <summary>
/// Starts the API against a real PostgreSQL container, shared by every test in the assembly.
/// Migrations run on startup, so the tests also check that they apply cleanly.
/// </summary>
public sealed class StoreApiFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _database = new PostgreSqlBuilder("postgres:17-alpine").Build();

    public async ValueTask InitializeAsync()
    {
        await _database.StartAsync();

        // Start the host once, before tests run in parallel, so migrations are applied a single time.
        _ = Services;
    }

    /// <summary>Connection string to a new, empty database on the same server, for tests that need isolation.</summary>
    public string ConnectionStringForNewDatabase() =>
        new NpgsqlConnectionStringBuilder(_database.GetConnectionString())
        {
            Database = $"test_{Guid.NewGuid():N}",
        }.ConnectionString;

    public override async ValueTask DisposeAsync()
    {
        await base.DisposeAsync();
        await _database.DisposeAsync();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.UseSetting("ConnectionStrings:Store", _database.GetConnectionString());
        builder.UseSetting("Database:ApplyMigrationsOnStartup", "true");
    }
}
