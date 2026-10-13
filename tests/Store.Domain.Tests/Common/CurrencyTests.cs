using Store.Domain.Common;

namespace Store.Domain.Tests.Common;

public class CurrencyTests
{
    [Theory]
    [InlineData("COP")]
    [InlineData("cop")]
    [InlineData(" cop ")]
    public void FromCode_is_case_insensitive(string code)
    {
        Assert.Same(Currency.Cop, Currency.FromCode(code));
    }

    [Fact]
    public void FromCode_rejects_unsupported_currency()
    {
        Assert.Throws<DomainException>(() => Currency.FromCode("EUR"));
    }

    [Fact]
    public void Cop_has_no_decimal_places()
    {
        Assert.Equal(0, Currency.Cop.DecimalPlaces);
        Assert.Equal(2, Currency.Usd.DecimalPlaces);
    }
}
