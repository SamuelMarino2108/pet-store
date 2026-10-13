using Store.Application.Common;

namespace Store.Application.Catalog;

/// <summary>Read side of the catalog, implemented by the persistence layer.</summary>
public interface ICatalogQueries
{
    Task<IReadOnlyList<CategoryDto>> GetCategoriesAsync(CancellationToken cancellationToken);

    Task<PagedResult<ProductSummaryDto>> SearchProductsAsync(
        ProductSearchCriteria criteria,
        CancellationToken cancellationToken);

    Task<ProductDetailDto?> GetProductBySlugAsync(string slug, CancellationToken cancellationToken);
}
