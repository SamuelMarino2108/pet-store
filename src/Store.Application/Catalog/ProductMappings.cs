using Store.Application.Common;
using Store.Domain.Catalog;

namespace Store.Application.Catalog;

/// <summary>
/// Maps catalog entities to DTOs. Prices come from the domain, so the API always shows
/// the same final price the checkout will charge.
/// </summary>
public static class ProductMappings
{
    public static ProductSummaryDto ToSummary(this Product product, Category category)
    {
        ArgumentNullException.ThrowIfNull(product);
        ArgumentNullException.ThrowIfNull(category);

        return new ProductSummaryDto(
            product.Id,
            product.Slug,
            product.Name,
            product.ImageUrl,
            category.Slug,
            category.Name,
            MoneyDto.From(product.Price),
            MoneyDto.From(product.FinalPrice),
            product.DiscountPercentage,
            product.IsInStock);
    }

    public static ProductDetailDto ToDetail(this Product product, Category category)
    {
        ArgumentNullException.ThrowIfNull(product);
        ArgumentNullException.ThrowIfNull(category);

        return new ProductDetailDto(
            product.Id,
            product.Slug,
            product.Sku.Value,
            product.Name,
            product.Description,
            product.ImageUrl,
            category.Slug,
            category.Name,
            MoneyDto.From(product.Price),
            MoneyDto.From(product.FinalPrice),
            product.DiscountPercentage,
            product.IsInStock);
    }
}
