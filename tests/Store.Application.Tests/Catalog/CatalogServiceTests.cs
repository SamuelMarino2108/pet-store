using FluentValidation;
using Store.Application.Catalog;
using Store.Application.Common;

namespace Store.Application.Tests.Catalog;

public class CatalogServiceTests
{
    private readonly FakeCatalogQueries _queries = new();
    private readonly CatalogService _service;
    private readonly CancellationToken _cancellationToken = TestContext.Current.CancellationToken;

    public CatalogServiceTests()
    {
        _service = new CatalogService(_queries, new ProductSearchCriteriaValidator());
    }

    [Fact]
    public async Task SearchProducts_rejects_invalid_criteria_without_querying()
    {
        await Assert.ThrowsAsync<ValidationException>(() =>
            _service.SearchProductsAsync(new ProductSearchCriteria { PageSize = 0 }, _cancellationToken));

        Assert.Null(_queries.LastCriteria);
    }

    [Fact]
    public async Task SearchProducts_normalizes_category_and_search()
    {
        var criteria = new ProductSearchCriteria { Category = "  Juguetes ", Search = "  pelota  " };

        await _service.SearchProductsAsync(criteria, _cancellationToken);

        Assert.Equal("juguetes", _queries.LastCriteria?.Category);
        Assert.Equal("pelota", _queries.LastCriteria?.Search);
    }

    [Fact]
    public async Task SearchProducts_treats_blank_filters_as_absent()
    {
        var criteria = new ProductSearchCriteria { Category = " ", Search = "" };

        await _service.SearchProductsAsync(criteria, _cancellationToken);

        Assert.Null(_queries.LastCriteria?.Category);
        Assert.Null(_queries.LastCriteria?.Search);
    }

    [Fact]
    public async Task GetProductBySlug_normalizes_slug()
    {
        await _service.GetProductBySlugAsync(" Cama-Ortopedica ", _cancellationToken);

        Assert.Equal("cama-ortopedica", _queries.LastSlug);
    }

    [Fact]
    public async Task GetProductBySlug_returns_null_for_blank_slug_without_querying()
    {
        var product = await _service.GetProductBySlugAsync("  ", _cancellationToken);

        Assert.Null(product);
        Assert.Null(_queries.LastSlug);
    }

    private sealed class FakeCatalogQueries : ICatalogQueries
    {
        public ProductSearchCriteria? LastCriteria { get; private set; }

        public string? LastSlug { get; private set; }

        public Task<IReadOnlyList<CategoryDto>> GetCategoriesAsync(CancellationToken cancellationToken) =>
            Task.FromResult<IReadOnlyList<CategoryDto>>([]);

        public Task<PagedResult<ProductSummaryDto>> SearchProductsAsync(
            ProductSearchCriteria criteria,
            CancellationToken cancellationToken)
        {
            LastCriteria = criteria;
            return Task.FromResult(new PagedResult<ProductSummaryDto>([], criteria.Page, criteria.PageSize, 0));
        }

        public Task<ProductDetailDto?> GetProductBySlugAsync(string slug, CancellationToken cancellationToken)
        {
            LastSlug = slug;
            return Task.FromResult<ProductDetailDto?>(null);
        }
    }
}
