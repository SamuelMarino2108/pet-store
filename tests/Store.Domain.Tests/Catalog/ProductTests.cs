using Store.Domain.Catalog;
using Store.Domain.Common;

namespace Store.Domain.Tests.Catalog;

public class ProductTests
{
    private static readonly Guid CategoryId = Guid.CreateVersion7();

    [Fact]
    public void Create_sets_all_fields()
    {
        var product = CreateProduct(cost: 20_000, price: 45_000, stock: 8);

        Assert.Equal("DOG-BED-M", product.Sku.Value);
        Assert.Equal("Cama acolchada mediana", product.Name);
        Assert.Equal(CategoryId, product.CategoryId);
        Assert.Equal(Money.Cop(20_000), product.Cost);
        Assert.Equal(Money.Cop(45_000), product.Price);
        Assert.Equal(8, product.Stock);
        Assert.Equal("simulated", product.Supplier.SupplierCode);
        Assert.Null(product.DiscountPercentage);
    }

    [Fact]
    public void Final_price_equals_price_without_discount()
    {
        var product = CreateProduct(price: 45_000);

        Assert.Equal(Money.Cop(45_000), product.FinalPrice);
    }

    [Fact]
    public void Discount_reduces_final_price_and_margin()
    {
        var product = CreateProduct(cost: 20_000, price: 45_000);

        product.ApplyDiscount(20);

        Assert.Equal(Money.Cop(36_000), product.FinalPrice);
        Assert.Equal(Money.Cop(16_000), product.GrossMargin);
    }

    [Fact]
    public void RemoveDiscount_restores_price()
    {
        var product = CreateProduct(price: 45_000);
        product.ApplyDiscount(20);

        product.RemoveDiscount();

        Assert.Null(product.DiscountPercentage);
        Assert.Equal(Money.Cop(45_000), product.FinalPrice);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(100)]
    [InlineData(-5)]
    public void ApplyDiscount_rejects_out_of_range_percentage(decimal percentage)
    {
        var product = CreateProduct();

        Assert.Throws<DomainException>(() => product.ApplyDiscount(percentage));
    }

    [Fact]
    public void ApplyDiscount_accepts_two_decimal_places()
    {
        var product = CreateProduct();

        product.ApplyDiscount(12.75m);

        Assert.Equal(12.75m, product.DiscountPercentage);
    }

    [Fact]
    public void ApplyDiscount_rejects_more_than_two_decimal_places()
    {
        var product = CreateProduct();

        Assert.Throws<DomainException>(() => product.ApplyDiscount(12.345m));
    }

    [Fact]
    public void Gross_margin_can_be_negative_when_selling_below_cost()
    {
        var product = CreateProduct(cost: 30_000, price: 25_000);

        Assert.Equal(Money.Cop(-5_000), product.GrossMargin);
    }

    [Theory]
    [InlineData(-1, 10_000)]
    [InlineData(5_000, 0)]
    [InlineData(5_000, -10_000)]
    public void Create_rejects_invalid_pricing(decimal cost, decimal price)
    {
        Assert.Throws<DomainException>(() => CreateProduct(cost: cost, price: price));
    }

    [Fact]
    public void Pricing_rejects_mixed_currencies()
    {
        var product = CreateProduct();

        Assert.Throws<DomainException>(() =>
            product.UpdatePricing(Money.Cop(10_000), Money.Of(20, Currency.Usd)));
    }

    [Fact]
    public void Create_rejects_negative_stock()
    {
        Assert.Throws<DomainException>(() => CreateProduct(stock: -1));
    }

    [Fact]
    public void Create_requires_a_category()
    {
        Assert.Throws<DomainException>(() => Product.Create(
            Sku.Create("DOG-BED-M"),
            "Cama",
            string.Empty,
            Guid.Empty,
            Money.Cop(10_000),
            Money.Cop(20_000),
            1,
            SupplierReference.Create("simulated", "SIM-1")));
    }

    [Fact]
    public void Create_rejects_name_longer_than_max_length()
    {
        Assert.Throws<DomainException>(() => CreateProduct(name: new string('a', Product.NameMaxLength + 1)));
    }

    [Fact]
    public void UpdateStock_changes_availability()
    {
        var product = CreateProduct(stock: 3);

        product.UpdateStock(0);

        Assert.False(product.IsInStock);
    }

    [Fact]
    public void UpdateDetails_does_not_change_state_when_invalid()
    {
        var product = CreateProduct();

        Assert.Throws<DomainException>(() => product.UpdateDetails("  ", "Nueva", CategoryId));
        Assert.Equal("Cama acolchada mediana", product.Name);
    }

    private static Product CreateProduct(
        decimal cost = 20_000,
        decimal price = 45_000,
        int stock = 10,
        string name = "Cama acolchada mediana") =>
        Product.Create(
            Sku.Create("dog-bed-m"),
            name,
            "Cama lavable para perros de 10 a 20 kg.",
            CategoryId,
            Money.Cop(cost),
            Money.Cop(price),
            stock,
            SupplierReference.Create("Simulated", "SIM-0001"));
}
