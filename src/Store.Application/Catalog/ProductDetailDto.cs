using Store.Application.Common;

namespace Store.Application.Catalog;

/// <summary>Product as shown on its detail page.</summary>
public sealed record ProductDetailDto(
    Guid Id,
    string Slug,
    string Sku,
    string Name,
    string Description,
    string ImageUrl,
    string CategorySlug,
    string CategoryName,
    MoneyDto Price,
    MoneyDto FinalPrice,
    decimal? DiscountPercentage,
    bool InStock);
