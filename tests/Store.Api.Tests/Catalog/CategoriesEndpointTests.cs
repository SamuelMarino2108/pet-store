using System.Net.Http.Json;
using Store.Api.Tests.Infrastructure;
using Store.Application.Catalog;

namespace Store.Api.Tests.Catalog;

public sealed class CategoriesEndpointTests(StoreApiFactory factory) : IDisposable
{
    private readonly HttpClient _client = factory.CreateClient();
    private readonly CancellationToken _cancellationToken = TestContext.Current.CancellationToken;

    public void Dispose() => _client.Dispose();

    [Fact]
    public async Task Lists_categories_with_product_count()
    {
        var catalog = new CatalogBuilder();
        catalog.AddProduct("Rascador", 30_000);
        catalog.AddProduct("Túnel", 40_000);
        await catalog.SaveAsync(factory, _cancellationToken);

        var categories = await _client.GetFromJsonAsync<List<CategoryDto>>(
            new Uri("/api/categories", UriKind.Relative),
            _cancellationToken);

        var category = Assert.Single(categories!, c => c.Id == catalog.Category.Id);
        Assert.Equal(catalog.Category.Slug, category.Slug);
        Assert.Equal(2, category.ProductCount);
    }
}
