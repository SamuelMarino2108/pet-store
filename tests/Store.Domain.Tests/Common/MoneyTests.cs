using System.Globalization;
using Store.Domain.Common;

namespace Store.Domain.Tests.Common;

public class MoneyTests
{
    [Fact]
    public void Of_rejects_more_decimals_than_the_currency_allows()
    {
        Assert.Throws<DomainException>(() => Money.Cop(1500.5m));
        Assert.Throws<DomainException>(() => Money.Of(10.123m, Currency.Usd));
    }

    [Fact]
    public void Of_accepts_amounts_within_currency_precision()
    {
        var money = Money.Of(10.25m, Currency.Usd);

        Assert.Equal(10.25m, money.Amount);
        Assert.Equal(Currency.Usd, money.Currency);
    }

    [Fact]
    public void Of_trims_scale_beyond_the_currency_decimal_places()
    {
        Assert.Equal("16900", Money.Cop(16_900.00m).Amount.ToString(CultureInfo.InvariantCulture));
    }

    [Fact]
    public void Equal_amounts_and_currency_are_equal()
    {
        Assert.Equal(Money.Cop(50_000), Money.Cop(50_000));
        Assert.NotEqual(Money.Cop(50_000), Money.Of(50_000, Currency.Usd));
    }

    [Fact]
    public void Add_and_subtract_same_currency()
    {
        var a = Money.Cop(30_000);
        var b = Money.Cop(12_000);

        Assert.Equal(Money.Cop(42_000), a + b);
        Assert.Equal(Money.Cop(18_000), a - b);
    }

    [Fact]
    public void Subtract_can_produce_a_negative_amount()
    {
        var result = Money.Cop(10_000) - Money.Cop(15_000);

        Assert.True(result.IsNegative);
        Assert.Equal(-5_000, result.Amount);
    }

    [Fact]
    public void Combining_different_currencies_throws()
    {
        Assert.Throws<DomainException>(() => Money.Cop(1_000) + Money.Of(1, Currency.Usd));
    }

    [Fact]
    public void Multiply_by_quantity()
    {
        Assert.Equal(Money.Cop(75_000), Money.Cop(25_000).Multiply(3));
    }

    [Theory]
    [InlineData(49_900, 10, 44_910)]
    [InlineData(10_001, 50, 5_001)] // 5000.5 rounds away from zero
    [InlineData(20_000, 0, 20_000)]
    [InlineData(20_000, 100, 0)]
    public void ApplyDiscount_rounds_to_currency_precision(decimal amount, decimal percentage, decimal expected)
    {
        Assert.Equal(Money.Cop(expected), Money.Cop(amount).ApplyDiscount(percentage));
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(101)]
    public void ApplyDiscount_rejects_out_of_range_percentage(decimal percentage)
    {
        Assert.Throws<DomainException>(() => Money.Cop(10_000).ApplyDiscount(percentage));
    }
}
