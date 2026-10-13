using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Store.Api.Tests.Infrastructure;
using Store.Infrastructure;
using Store.Infrastructure.Persistence;

namespace Store.Api.Tests.Persistence;

/// <summary>
/// The seed only runs on an empty catalog, so it gets its own database instead of the shared one.
/// </summary>
public class SampleDataSeedTests(StoreApiFactory factory)
{
    private readonly CancellationToken _cancellationToken = TestContext.Current.CancellationToken;

    [Fact]
    public async Task Seed_loads_the_sample_catalog_once()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:Store"] = factory.ConnectionStringForNewDatabase(),
            })
            .Build();

        await using var services = new ServiceCollection()
            .AddLogging()
            .AddInfrastructure(configuration)
            .BuildServiceProvider();

        await services.ApplyMigrationsAsync(_cancellationToken);
        await services.SeedSampleDataAsync(_cancellationToken);
        await services.SeedSampleDataAsync(_cancellationToken);

        await using var scope = services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<StoreDbContext>();
        Assert.Equal(6, await dbContext.Categories.CountAsync(_cancellationToken));
        Assert.Equal(30, await dbContext.Products.CountAsync(_cancellationToken));
    }
}
