using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Store.Api.Tests.Infrastructure;
using Store.Domain.Catalog;
using Store.Domain.Common;
using Store.Infrastructure.Persistence;

namespace Store.Api.Tests.Persistence;

public class CatalogPersistenceTests(StoreApiFactory factory)
{
    private readonly CancellationToken _cancellationToken = TestContext.Current.CancellationToken;

    [Fact]
    public async Task Product_round_trips_with_its_value_objects()
    {
        var category = Category.Create($"Camas {Guid.NewGuid():N}");
        var product = Product.Create(
            Sku.Create($"BED-{Guid.NewGuid():N}"[..20]),
            "Cama acolchada mediana",
            "Cama lavable para perros de 10 a 20 kg.",
            category.Id,
            Money.Cop(20_000),
            Money.Cop(45_000),
            stock: 8,
            SupplierReference.Create("simulated", "SIM-0001"));
        product.ApplyDiscount(12.5m);

        await using (var scope = factory.Services.CreateAsyncScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<StoreDbContext>();
            dbContext.AddRange(category, product);
            await dbContext.SaveChangesAsync(_cancellationToken);
        }

        await using (var scope = factory.Services.CreateAsyncScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<StoreDbContext>();
            var loaded = await dbContext.Products.SingleAsync(p => p.Id == product.Id, _cancellationToken);

            Assert.Equal(product.Sku, loaded.Sku);
            Assert.Equal(category.Id, loaded.CategoryId);
            Assert.Equal(Money.Cop(20_000), loaded.Cost);
            Assert.Equal(Money.Cop(45_000), loaded.Price);
            Assert.Equal(12.5m, loaded.DiscountPercentage);
            Assert.Equal(Money.Cop(39_375), loaded.FinalPrice);
            Assert.Equal(8, loaded.Stock);
            Assert.Equal(SupplierReference.Create("simulated", "SIM-0001"), loaded.Supplier);
        }
    }

    [Fact]
    public async Task Category_slug_must_be_unique()
    {
        var name = $"Collares {Guid.NewGuid():N}";

        await using var scope = factory.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<StoreDbContext>();
        dbContext.AddRange(Category.Create(name), Category.Create(name));

        await Assert.ThrowsAsync<DbUpdateException>(() => dbContext.SaveChangesAsync(_cancellationToken));
    }
}
