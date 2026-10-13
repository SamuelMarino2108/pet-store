namespace Store.Domain.Common;

/// <summary>
/// An amount in a given currency. Uses <see cref="decimal"/> to avoid floating point errors
/// and refuses to mix currencies. Negative amounts are allowed so that results such as
/// a margin can be expressed; entities decide when an amount must be positive.
/// </summary>
public sealed record Money
{
    private Money(decimal amount, Currency currency)
    {
        Amount = amount;
        Currency = currency;
    }

    public decimal Amount { get; }

    public Currency Currency { get; }

    public bool IsNegative => Amount < 0;

    public bool IsZero => Amount == 0;

    public static Money Of(decimal amount, Currency currency)
    {
        ArgumentNullException.ThrowIfNull(currency);

        if (decimal.Round(amount, currency.DecimalPlaces) != amount)
        {
            throw new DomainException(
                $"{currency.Code} amounts allow at most {currency.DecimalPlaces} decimal places.");
        }

        return new Money(amount, currency);
    }

    public static Money Cop(decimal amount) => Of(amount, Currency.Cop);

    public static Money Zero(Currency currency) => Of(0m, currency);

    public static Money operator +(Money left, Money right)
    {
        ArgumentNullException.ThrowIfNull(left);
        return left.Add(right);
    }

    public static Money operator -(Money left, Money right)
    {
        ArgumentNullException.ThrowIfNull(left);
        return left.Subtract(right);
    }

    public Money Add(Money other)
    {
        EnsureSameCurrency(other);
        return new Money(Amount + other.Amount, Currency);
    }

    public Money Subtract(Money other)
    {
        EnsureSameCurrency(other);
        return new Money(Amount - other.Amount, Currency);
    }

    public Money Multiply(int quantity) => new(Amount * quantity, Currency);

    /// <summary>
    /// Returns the amount reduced by <paramref name="percentage"/> (0-100), rounded half away
    /// from zero to the decimal places of the currency.
    /// </summary>
    public Money ApplyDiscount(decimal percentage)
    {
        if (percentage is < 0 or > 100)
        {
            throw new DomainException("Discount percentage must be between 0 and 100.");
        }

        var discounted = Amount * (100 - percentage) / 100;
        var rounded = decimal.Round(discounted, Currency.DecimalPlaces, MidpointRounding.AwayFromZero);

        return new Money(rounded, Currency);
    }

    public override string ToString() => $"{Amount} {Currency.Code}";

    private void EnsureSameCurrency(Money other)
    {
        ArgumentNullException.ThrowIfNull(other);

        if (other.Currency != Currency)
        {
            throw new DomainException($"Cannot combine {Currency.Code} with {other.Currency.Code}.");
        }
    }
}
