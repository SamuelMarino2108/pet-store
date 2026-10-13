using Store.Domain.Common;

namespace Store.Domain.Catalog;

/// <summary>
/// A sellable unit of the catalog. The admin sets the sale price by hand; the cost comes from
/// the supplier. Final price and margin are calculated, never stored.
/// </summary>
public sealed class Product : Entity
{
    public const int NameMaxLength = 200;
    public const int DescriptionMaxLength = 4000;
    public const int ImageUrlMaxLength = 500;
    public const int DiscountDecimalPlaces = 2;

    // Required by EF Core to materialize the entity.
    private Product()
    {
        Sku = null!;
        Name = null!;
        Slug = null!;
        Description = null!;
        ImageUrl = null!;
        Cost = null!;
        Price = null!;
        Supplier = null!;
    }

    private Product(
        Sku sku,
        string name,
        string description,
        string imageUrl,
        Guid categoryId,
        Money cost,
        Money price,
        int stock,
        SupplierReference supplier)
    {
        Sku = sku;
        Name = name;
        Slug = SlugGenerator.FromName(name, nameof(Product));
        Description = description;
        ImageUrl = imageUrl;
        CategoryId = categoryId;
        Cost = cost;
        Price = price;
        Stock = stock;
        Supplier = supplier;
    }

    public Sku Sku { get; private set; }

    public string Name { get; private set; }

    /// <summary>URL-friendly identifier derived from the name, unique across products.</summary>
    public string Slug { get; private set; }

    public string Description { get; private set; }

    /// <summary>Main image: an absolute http(s) URL or a path relative to the site root.</summary>
    public string ImageUrl { get; private set; }

    /// <summary>Categories are a separate aggregate, so they are referenced by id.</summary>
    public Guid CategoryId { get; private set; }

    /// <summary>What the supplier charges for the product.</summary>
    public Money Cost { get; private set; }

    /// <summary>Sale price before discount.</summary>
    public Money Price { get; private set; }

    /// <summary>Manual discount set by the admin, greater than 0 and less than 100.</summary>
    public decimal? DiscountPercentage { get; private set; }

    /// <summary>Local copy of the supplier's stock.</summary>
    public int Stock { get; private set; }

    public SupplierReference Supplier { get; private set; }

    public Money FinalPrice => DiscountPercentage is { } percentage ? Price.ApplyDiscount(percentage) : Price;

    /// <summary>Final price minus product cost. Shipping and gateway fees are not included.</summary>
    public Money GrossMargin => FinalPrice - Cost;

    public bool IsInStock => Stock > 0;

    public static Product Create(
        Sku sku,
        string name,
        string description,
        string imageUrl,
        Guid categoryId,
        Money cost,
        Money price,
        int stock,
        SupplierReference supplier)
    {
        ArgumentNullException.ThrowIfNull(sku);
        ArgumentNullException.ThrowIfNull(supplier);
        EnsureValidCategory(categoryId);
        EnsureValidPricing(cost, price);
        EnsureValidStock(stock);

        return new Product(
            sku,
            Guard.NotEmpty(name, nameof(name), NameMaxLength),
            NormalizeDescription(description),
            ValidateImageUrl(imageUrl),
            categoryId,
            cost,
            price,
            stock,
            supplier);
    }

    public void UpdateDetails(string name, string description, Guid categoryId)
    {
        EnsureValidCategory(categoryId);
        var validName = Guard.NotEmpty(name, nameof(name), NameMaxLength);
        var validDescription = NormalizeDescription(description);
        var slug = SlugGenerator.FromName(validName, nameof(Product));

        Name = validName;
        Slug = slug;
        Description = validDescription;
        CategoryId = categoryId;
    }

    public void ChangeImage(string imageUrl) => ImageUrl = ValidateImageUrl(imageUrl);

    public void UpdatePricing(Money cost, Money price)
    {
        EnsureValidPricing(cost, price);

        Cost = cost;
        Price = price;
    }

    public void ApplyDiscount(decimal percentage)
    {
        if (percentage is <= 0 or >= 100)
        {
            throw new DomainException("Discount percentage must be greater than 0 and less than 100.");
        }

        if (decimal.Round(percentage, DiscountDecimalPlaces) != percentage)
        {
            throw new DomainException(
                $"Discount percentage allows at most {DiscountDecimalPlaces} decimal places.");
        }

        DiscountPercentage = percentage;
    }

    public void RemoveDiscount() => DiscountPercentage = null;

    public void UpdateStock(int stock)
    {
        EnsureValidStock(stock);
        Stock = stock;
    }

    private static void EnsureValidCategory(Guid categoryId)
    {
        if (categoryId == Guid.Empty)
        {
            throw new DomainException("Product must belong to a category.");
        }
    }

    private static void EnsureValidPricing(Money cost, Money price)
    {
        ArgumentNullException.ThrowIfNull(cost);
        ArgumentNullException.ThrowIfNull(price);

        if (cost.Currency != price.Currency)
        {
            throw new DomainException("Cost and price must use the same currency.");
        }

        if (cost.IsNegative)
        {
            throw new DomainException("Cost cannot be negative.");
        }

        if (price.IsNegative || price.IsZero)
        {
            throw new DomainException("Price must be greater than zero.");
        }
    }

    private static void EnsureValidStock(int stock)
    {
        if (stock < 0)
        {
            throw new DomainException("Stock cannot be negative.");
        }
    }

    private static string ValidateImageUrl(string imageUrl)
    {
        var url = Guard.NotEmpty(imageUrl, nameof(imageUrl), ImageUrlMaxLength);
        var isSitePath = url.StartsWith('/') && !url.StartsWith("//", StringComparison.Ordinal);
        var isWebUrl = Uri.TryCreate(url, UriKind.Absolute, out var uri)
            && (uri.Scheme == Uri.UriSchemeHttps || uri.Scheme == Uri.UriSchemeHttp);

        return isSitePath || isWebUrl
            ? url
            : throw new DomainException("Image URL must be an http(s) URL or a path starting with '/'.");
    }

    private static string NormalizeDescription(string? description)
    {
        var trimmed = description?.Trim() ?? string.Empty;
        return trimmed.Length <= DescriptionMaxLength
            ? trimmed
            : throw new DomainException($"Description must be at most {DescriptionMaxLength} characters long.");
    }
}
