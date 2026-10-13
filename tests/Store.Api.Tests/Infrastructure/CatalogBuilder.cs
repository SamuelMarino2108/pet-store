using Microsoft.Extensions.DependencyInjection;
using Store.Domain.Catalog;
using Store.Domain.Common;
using Store.Infrastructure.Persistence;

namespace Store.Api.Tests.Infrastructure;

/// <summary>
/// Creates a category with products under unique names, so each test works with its own slice
/// of the shared database and can filter the catalog by that category.
/// </summary>
internal sealed class CatalogBuilder
{
    private readonly string _token = Guid.NewGuid().ToString("N")[..12];
    private readonly List<Product> _products = [];

    public CatalogBuilder() => Category = Category.Create($"Categoria {_token}");

    public Category Category { get; }

    public Product AddProduct(string name, decimal price, int stock = 10, decimal? discount = null)
    {
        var sku = Sku.Create($"T-{_token}-{_products.Count + 1}");
        var product = Product.Create(
            sku,
            $"{name} {_token}",
            "Producto de prueba.",
            "/images/products/test.webp",
            Category.Id,
            Money.Cop(price / 2),
            Money.Cop(price),
            stock,
            SupplierReference.Create("simulated", sku.Value));

        if (discount is { } percentage)
        {
            product.ApplyDiscount(percentage);
        }

        _products.Add(product);
        return product;
    }

    public async Task SaveAsync(StoreApiFactory factory, CancellationToken cancellationToken)
    {
        await using var scope = factory.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<StoreDbContext>();
        dbContext.Categories.Add(Category);
        dbContext.Products.AddRange(_products);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
