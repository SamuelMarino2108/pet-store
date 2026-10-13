using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Store.Application.Catalog;
using Store.Infrastructure.Persistence;
using Store.Infrastructure.Persistence.Queries;
using Store.Infrastructure.Persistence.Seeding;

namespace Store.Infrastructure;

public static class DependencyInjection
{
    public const string ConnectionStringName = "Store";

    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString(ConnectionStringName);
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException($"Connection string '{ConnectionStringName}' is not configured.");
        }

        services.AddDbContext<StoreDbContext>(options => options
            .UseNpgsql(connectionString)
            .UseSnakeCaseNamingConvention());

        services.AddScoped<ICatalogQueries, CatalogQueries>();

        return services;
    }

    /// <summary>
    /// Applies pending migrations. Meant for local development and the demo environment;
    /// a production deployment would run migrations as a separate step.
    /// </summary>
    public static async Task ApplyMigrationsAsync(this IServiceProvider services, CancellationToken cancellationToken = default)
    {
        await using var scope = services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<StoreDbContext>();
        await dbContext.Database.MigrateAsync(cancellationToken);
    }

    /// <summary>Loads the sample catalog when the database has no categories yet.</summary>
    public static async Task SeedSampleDataAsync(this IServiceProvider services, CancellationToken cancellationToken = default)
    {
        await using var scope = services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<StoreDbContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger(typeof(CatalogSeeder));

        var productCount = await CatalogSeeder.SeedAsync(dbContext, cancellationToken);
        if (productCount > 0)
        {
            SeedingLog.CatalogSeeded(logger, productCount);
        }
    }
}
