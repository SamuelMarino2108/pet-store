using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Store.Domain.Catalog;
using Store.Domain.Common;

namespace Store.Infrastructure.Persistence.Configurations;

internal sealed class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    /// <summary>
    /// Shadow property with the price after discount, kept by PostgreSQL as a stored generated
    /// column so the catalog can filter, sort and index by it. The domain remains the source of
    /// truth for <see cref="Product.FinalPrice"/>; the SQL mirrors its rounding (half away from zero
    /// to the decimal places of the currency).
    /// </summary>
    public const string FinalPriceAmount = "FinalPriceAmount";

    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id).ValueGeneratedNever();

        builder.Property(p => p.Sku)
            .HasConversion(sku => sku.Value, value => Sku.Create(value))
            .HasMaxLength(Sku.MaxLength);
        builder.HasIndex(p => p.Sku).IsUnique();

        builder.Property(p => p.Name).HasMaxLength(Product.NameMaxLength);
        builder.Property(p => p.Slug).HasMaxLength(Product.NameMaxLength);
        builder.HasIndex(p => p.Slug).IsUnique();
        builder.Property(p => p.Description).HasMaxLength(Product.DescriptionMaxLength);
        builder.Property(p => p.ImageUrl).HasMaxLength(Product.ImageUrlMaxLength);

        // Trigram index for case-insensitive "contains" search on the name.
        builder.HasIndex(p => p.Name)
            .HasMethod("gin")
            .HasOperators("gin_trgm_ops");
        builder.Property(p => p.DiscountPercentage).HasPrecision(5, Product.DiscountDecimalPlaces);

        builder.ComplexProperty(p => p.Cost, MoneyMapping.Configure);
        builder.ComplexProperty(p => p.Price, MoneyMapping.Configure);

        builder.ComplexProperty(p => p.Supplier, supplier =>
        {
            supplier.Property(s => s.SupplierCode)
                .HasColumnName("supplier_code")
                .HasMaxLength(SupplierReference.SupplierCodeMaxLength);
            supplier.Property(s => s.SupplierProductId)
                .HasColumnName("supplier_product_id")
                .HasMaxLength(SupplierReference.SupplierProductIdMaxLength);
        });

        // Categories with products cannot be deleted by accident.
        builder.HasOne<Category>()
            .WithMany()
            .HasForeignKey(p => p.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property<decimal>(FinalPriceAmount)
            .HasPrecision(18, 2)
            .HasComputedColumnSql(FinalPriceSql(), stored: true);
        builder.HasIndex(nameof(Product.CategoryId), FinalPriceAmount);

        builder.Ignore(p => p.FinalPrice);
        builder.Ignore(p => p.GrossMargin);
        builder.Ignore(p => p.IsInStock);
    }

    private static string FinalPriceSql()
    {
        var decimalPlaces = string.Concat(Currency.All.Select(c => $"WHEN '{c.Code}' THEN {c.DecimalPlaces} "));
        return "round(price_amount * (100 - coalesce(discount_percentage, 0)) / 100, "
            + $"CASE price_currency {decimalPlaces}END)";
    }
}
