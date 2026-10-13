using Store.Application.Common;

namespace Store.Application.Catalog;

/// <summary>Product as shown in the catalog list. Cost and margin are never exposed publicly.</summary>
public sealed record ProductSummaryDto(
    Guid Id,
    string Slug,
    string Name,
    string ImageUrl,
    string CategorySlug,
    string CategoryName,
    MoneyDto Price,
    MoneyDto FinalPrice,
    decimal? DiscountPercentage,
    bool InStock);
