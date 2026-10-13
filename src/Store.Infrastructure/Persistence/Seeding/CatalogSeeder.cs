using System.Globalization;
using Microsoft.EntityFrameworkCore;
using Store.Domain.Catalog;
using Store.Domain.Common;

namespace Store.Infrastructure.Persistence.Seeding;

/// <summary>
/// Loads the sample catalog through the domain factories, so seed data follows the same rules
/// as data entered by the admin. It only runs on an empty catalog, so it is safe to call on every start.
/// </summary>
internal static class CatalogSeeder
{
    private const string SupplierCode = "simulated";

    public static async Task<int> SeedAsync(StoreDbContext dbContext, CancellationToken cancellationToken)
    {
        if (await dbContext.Categories.AnyAsync(cancellationToken))
        {
            return 0;
        }

        var productCount = 0;

        foreach (var seedCategory in CatalogSeedData.Categories)
        {
            var category = Category.Create(seedCategory.Name);
            dbContext.Categories.Add(category);

            for (var i = 0; i < seedCategory.Products.Count; i++)
            {
                dbContext.Products.Add(CreateProduct(seedCategory, seedCategory.Products[i], i + 1, category.Id));
                productCount++;
            }
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        return productCount;
    }

    private static Product CreateProduct(
        CatalogSeedData.SeedCategory seedCategory,
        CatalogSeedData.SeedProduct seed,
        int number,
        Guid categoryId)
    {
        var sku = Sku.Create(string.Create(CultureInfo.InvariantCulture, $"{seedCategory.SkuPrefix}-{number:D3}"));

        var product = Product.Create(
            sku,
            seed.Name,
            seed.Description,
            $"/images/products/{sku.Value.ToLowerInvariant()}.webp",
            categoryId,
            Money.Cop(seed.Cost),
            Money.Cop(seed.Price),
            seed.Stock,
            SupplierReference.Create(SupplierCode, $"SIM-{sku.Value}"));

        if (seed.DiscountPercentage is { } discount)
        {
            product.ApplyDiscount(discount);
        }

        return product;
    }
}
