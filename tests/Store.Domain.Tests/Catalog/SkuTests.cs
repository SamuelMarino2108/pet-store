using Store.Domain.Catalog;
using Store.Domain.Common;

namespace Store.Domain.Tests.Catalog;

public class SkuTests
{
    [Fact]
    public void Create_normalizes_to_upper_case()
    {
        Assert.Equal("DOG-COLLAR-01", Sku.Create("  dog-collar-01 ").Value);
    }

    [Theory]
    [InlineData("")]
    [InlineData("AB")]
    [InlineData("-ABC")]
    [InlineData("DOG COLLAR")]
    [InlineData("DOG_COLLAR")]
    public void Create_rejects_invalid_values(string value)
    {
        Assert.Throws<DomainException>(() => Sku.Create(value));
    }

    [Fact]
    public void Create_rejects_values_longer_than_max_length()
    {
        Assert.Throws<DomainException>(() => Sku.Create(new string('A', Sku.MaxLength + 1)));
    }
}
