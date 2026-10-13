using Store.Application.Common;

namespace Store.Application.Catalog;

/// <summary>Public catalog use cases: browse categories, search products and view a product.</summary>
public interface ICatalogService
{
    Task<IReadOnlyList<CategoryDto>> GetCategoriesAsync(CancellationToken cancellationToken);

    /// <exception cref="FluentValidation.ValidationException">The criteria are invalid.</exception>
    Task<PagedResult<ProductSummaryDto>> SearchProductsAsync(
        ProductSearchCriteria criteria,
        CancellationToken cancellationToken);

    /// <returns>The product, or <see langword="null"/> when no product has that slug.</returns>
    Task<ProductDetailDto?> GetProductBySlugAsync(string slug, CancellationToken cancellationToken);
}
