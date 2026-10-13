using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Store.Api.Tests.Infrastructure;
using Store.Application.Catalog;
using Store.Application.Common;

namespace Store.Api.Tests.Catalog;

public sealed class ProductsEndpointTests(StoreApiFactory factory) : IDisposable
{
    private readonly HttpClient _client = factory.CreateClient();
    private readonly CancellationToken _cancellationToken = TestContext.Current.CancellationToken;

    public void Dispose() => _client.Dispose();

    [Fact]
    public async Task Filters_by_category_and_sorts_by_final_price()
    {
        var catalog = new CatalogBuilder();
        var cheapAfterDiscount = catalog.AddProduct("Cama", 60_000, discount: 50);
        var middle = catalog.AddProduct("Collar", 40_000);
        var expensive = catalog.AddProduct("Fuente", 80_000);
        await catalog.SaveAsync(factory, _cancellationToken);

        var page = await SearchAsync($"category={catalog.Category.Slug}&sort=PriceAsc");

        Assert.Equal(3, page.TotalCount);
        Assert.Equal([cheapAfterDiscount.Id, middle.Id, expensive.Id], page.Items.Select(p => p.Id));
        Assert.Equal(new MoneyDto(30_000, "COP"), page.Items[0].FinalPrice);
        Assert.Equal(new MoneyDto(60_000, "COP"), page.Items[0].Price);
        Assert.Equal(catalog.Category.Name, page.Items[0].CategoryName);
    }

    [Fact]
    public async Task Search_is_case_insensitive_and_treats_wildcards_as_text()
    {
        var catalog = new CatalogBuilder();
        var ball = catalog.AddProduct("Pelota dispensadora", 20_000);
        catalog.AddProduct("Cuerda", 15_000);
        await catalog.SaveAsync(factory, _cancellationToken);

        var matches = await SearchAsync($"category={catalog.Category.Slug}&search=PELOTA");
        var wildcard = await SearchAsync($"category={catalog.Category.Slug}&search=%25");

        Assert.Equal([ball.Id], matches.Items.Select(p => p.Id));
        Assert.Empty(wildcard.Items);
    }

    [Fact]
    public async Task Filters_by_final_price_range_and_stock()
    {
        var catalog = new CatalogBuilder();
        catalog.AddProduct("Barato", 10_000);
        var inRange = catalog.AddProduct("Medio", 50_000, discount: 20);
        catalog.AddProduct("Agotado", 40_000, stock: 0);
        catalog.AddProduct("Caro", 90_000);
        await catalog.SaveAsync(factory, _cancellationToken);

        var page = await SearchAsync(
            $"category={catalog.Category.Slug}&minPrice=20000&maxPrice=40000&inStockOnly=true");

        Assert.Equal([inRange.Id], page.Items.Select(p => p.Id));
    }

    [Fact]
    public async Task Pages_through_results_in_a_stable_order()
    {
        var catalog = new CatalogBuilder();
        var first = catalog.AddProduct("Primero", 10_000);
        var second = catalog.AddProduct("Segundo", 10_000);
        var third = catalog.AddProduct("Tercero", 10_000);
        await catalog.SaveAsync(factory, _cancellationToken);

        var page1 = await SearchAsync($"category={catalog.Category.Slug}&pageSize=2&page=1&sort=NameAsc");
        var page2 = await SearchAsync($"category={catalog.Category.Slug}&pageSize=2&page=2&sort=NameAsc");

        Assert.Equal([first.Id, second.Id], page1.Items.Select(p => p.Id));
        Assert.Equal([third.Id], page2.Items.Select(p => p.Id));
        Assert.Equal(3, page2.TotalCount);
        Assert.Equal(2, page2.TotalPages);
    }

    [Fact]
    public async Task Invalid_criteria_return_validation_problem()
    {
        var response = await _client.GetAsync(
            new Uri("/api/products?pageSize=500&minPrice=10&maxPrice=5", UriKind.Relative),
            _cancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var problem = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>(_cancellationToken);
        Assert.NotNull(problem);
        Assert.Contains(nameof(ProductSearchCriteria.PageSize), problem.Errors.Keys);
        Assert.Contains(nameof(ProductSearchCriteria.MaxPrice), problem.Errors.Keys);
    }

    [Fact]
    public async Task Unknown_sort_returns_bad_request()
    {
        var response = await _client.GetAsync(new Uri("/api/products?sort=Cheapest", UriKind.Relative), _cancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Gets_product_detail_by_slug_without_exposing_cost()
    {
        var catalog = new CatalogBuilder();
        var product = catalog.AddProduct("Arnés acolchado", 84_900, discount: 10);
        await catalog.SaveAsync(factory, _cancellationToken);

        var response = await _client.GetAsync(new Uri($"/api/products/{product.Slug}", UriKind.Relative), _cancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var json = await response.Content.ReadAsStringAsync(_cancellationToken);
        var detail = JsonSerializer.Deserialize<ProductDetailDto>(json, JsonSerializerOptions.Web);
        Assert.NotNull(detail);
        Assert.Equal(product.Id, detail.Id);
        Assert.Equal(product.Sku.Value, detail.Sku);
        Assert.Equal(new MoneyDto(76_410, "COP"), detail.FinalPrice);
        Assert.DoesNotContain("cost", json, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("margin", json, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Unknown_slug_returns_not_found_problem()
    {
        var response = await _client.GetAsync(new Uri("/api/products/does-not-exist", UriKind.Relative), _cancellationToken);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
    }

    private async Task<PagedResult<ProductSummaryDto>> SearchAsync(string query)
    {
        var page = await _client.GetFromJsonAsync<PagedResult<ProductSummaryDto>>(
            new Uri($"/api/products?{query}", UriKind.Relative),
            _cancellationToken);

        Assert.NotNull(page);
        return page;
    }
}
