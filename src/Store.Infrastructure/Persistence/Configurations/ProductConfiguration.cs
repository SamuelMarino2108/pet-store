using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Store.Domain.Catalog;

namespace Store.Infrastructure.Persistence.Configurations;

internal sealed class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id).ValueGeneratedNever();

        builder.Property(p => p.Sku)
            .HasConversion(sku => sku.Value, value => Sku.Create(value))
            .HasMaxLength(Sku.MaxLength);
        builder.HasIndex(p => p.Sku).IsUnique();

        builder.Property(p => p.Name).HasMaxLength(Product.NameMaxLength);
        builder.Property(p => p.Description).HasMaxLength(Product.DescriptionMaxLength);
        builder.Property(p => p.DiscountPercentage).HasPrecision(5, 2);

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

        builder.Ignore(p => p.FinalPrice);
        builder.Ignore(p => p.GrossMargin);
        builder.Ignore(p => p.IsInStock);
    }
}
