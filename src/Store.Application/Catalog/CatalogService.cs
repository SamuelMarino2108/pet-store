using FluentValidation;
using Store.Application.Common;

namespace Store.Application.Catalog;

internal sealed class CatalogService(
    ICatalogQueries queries,
    IValidator<ProductSearchCriteria> criteriaValidator) : ICatalogService
{
    public Task<IReadOnlyList<CategoryDto>> GetCategoriesAsync(CancellationToken cancellationToken) =>
        queries.GetCategoriesAsync(cancellationToken);

    public async Task<PagedResult<ProductSummaryDto>> SearchProductsAsync(
        ProductSearchCriteria criteria,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(criteria);
        await criteriaValidator.ValidateAndThrowAsync(criteria, cancellationToken);

        var normalized = criteria with
        {
            Category = NormalizeSlug(criteria.Category),
            Search = string.IsNullOrWhiteSpace(criteria.Search) ? null : criteria.Search.Trim(),
        };

        return await queries.SearchProductsAsync(normalized, cancellationToken);
    }

    public Task<ProductDetailDto?> GetProductBySlugAsync(string slug, CancellationToken cancellationToken) =>
        NormalizeSlug(slug) is { } normalized
            ? queries.GetProductBySlugAsync(normalized, cancellationToken)
            : Task.FromResult<ProductDetailDto?>(null);

    // Slugs are stored lower case; matching them that way lets the database use the unique index.
    private static string? NormalizeSlug(string? slug) =>
        string.IsNullOrWhiteSpace(slug) ? null : slug.Trim().ToLowerInvariant();
}
