namespace Store.Application.Catalog;

/// <summary>Filters, sorting and paging for the product list. Prices filter by final price.</summary>
public sealed record ProductSearchCriteria
{
    public const int DefaultPageSize = 12;
    public const int MaxPageSize = 48;
    public const int MaxPage = 10_000;
    public const int SearchMaxLength = 100;

    /// <summary>Category slug.</summary>
    public string? Category { get; init; }

    /// <summary>Text contained in the product name, case-insensitive.</summary>
    public string? Search { get; init; }

    public decimal? MinPrice { get; init; }

    public decimal? MaxPrice { get; init; }

    /// <summary>When true, only products with stock are returned.</summary>
    public bool InStockOnly { get; init; }

    public ProductSort Sort { get; init; } = ProductSort.Newest;

    public int Page { get; init; } = 1;

    public int PageSize { get; init; } = DefaultPageSize;
}
