using Microsoft.EntityFrameworkCore;
using Store.Application.Catalog;
using Store.Application.Common;
using Store.Domain.Catalog;
using Store.Infrastructure.Persistence.Configurations;

namespace Store.Infrastructure.Persistence.Queries;

/// <summary>
/// Catalog reads with EF Core. Filtering, sorting and paging run in PostgreSQL; only the
/// requested page is materialized and mapped, so prices are calculated by the domain.
/// </summary>
internal sealed class CatalogQueries(StoreDbContext dbContext) : ICatalogQueries
{
    private const string FinalPriceAmount = ProductConfiguration.FinalPriceAmount;

    public async Task<IReadOnlyList<CategoryDto>> GetCategoriesAsync(CancellationToken cancellationToken) =>
        await dbContext.Categories
            .AsNoTracking()
            .OrderBy(c => c.Name)
            .Select(c => new CategoryDto(
                c.Id,
                c.Slug,
                c.Name,
                dbContext.Products.Count(p => p.CategoryId == c.Id)))
            .ToListAsync(cancellationToken);

    public async Task<PagedResult<ProductSummaryDto>> SearchProductsAsync(
        ProductSearchCriteria criteria,
        CancellationToken cancellationToken)
    {
        var query = Filter(criteria);
        var totalCount = await query.CountAsync(cancellationToken);

        var page = await Sort(query, criteria.Sort)
            .Skip((criteria.Page - 1) * criteria.PageSize)
            .Take(criteria.PageSize)
            .ToListAsync(cancellationToken);

        var items = page.Select(row => row.Product.ToSummary(row.Category)).ToList();
        return new PagedResult<ProductSummaryDto>(items, criteria.Page, criteria.PageSize, totalCount);
    }

    public async Task<ProductDetailDto?> GetProductBySlugAsync(string slug, CancellationToken cancellationToken)
    {
        var row = await ProductsWithCategory()
            .Where(row => row.Product.Slug == slug)
            .SingleOrDefaultAsync(cancellationToken);

        return row?.Product.ToDetail(row.Category);
    }

    private IQueryable<ProductRow> ProductsWithCategory() =>
        dbContext.Products
            .AsNoTracking()
            .Join(
                dbContext.Categories.AsNoTracking(),
                product => product.CategoryId,
                category => category.Id,
                (product, category) => new ProductRow { Product = product, Category = category });

    private IQueryable<ProductRow> Filter(ProductSearchCriteria criteria)
    {
        var query = ProductsWithCategory();

        if (criteria.Category is { } categorySlug)
        {
            query = query.Where(row => row.Category.Slug == categorySlug);
        }

        if (criteria.Search is { } search)
        {
            var pattern = $"%{EscapeLikePattern(search)}%";
            query = query.Where(row => EF.Functions.ILike(row.Product.Name, pattern));
        }

        if (criteria.MinPrice is { } minPrice)
        {
            query = query.Where(row => EF.Property<decimal>(row.Product, FinalPriceAmount) >= minPrice);
        }

        if (criteria.MaxPrice is { } maxPrice)
        {
            query = query.Where(row => EF.Property<decimal>(row.Product, FinalPriceAmount) <= maxPrice);
        }

        if (criteria.InStockOnly)
        {
            query = query.Where(row => row.Product.Stock > 0);
        }

        return query;
    }

    // Ids are UUID v7, so ordering by id is ordering by creation time. The id is also the
    // tie-breaker that keeps pages stable when several products share a price or a name.
    private static IQueryable<ProductRow> Sort(IQueryable<ProductRow> query, ProductSort sort) => sort switch
    {
        ProductSort.PriceAsc => query.OrderBy(row => EF.Property<decimal>(row.Product, FinalPriceAmount)).ThenBy(row => row.Product.Id),
        ProductSort.PriceDesc => query.OrderByDescending(row => EF.Property<decimal>(row.Product, FinalPriceAmount)).ThenBy(row => row.Product.Id),
        ProductSort.NameAsc => query.OrderBy(row => row.Product.Name).ThenBy(row => row.Product.Id),
        _ => query.OrderByDescending(row => row.Product.Id),
    };

    private static string EscapeLikePattern(string value) =>
        value.Replace(@"\", @"\\", StringComparison.Ordinal)
            .Replace("%", @"\%", StringComparison.Ordinal)
            .Replace("_", @"\_", StringComparison.Ordinal);

    private sealed class ProductRow
    {
        public required Product Product { get; init; }

        public required Category Category { get; init; }
    }
}
