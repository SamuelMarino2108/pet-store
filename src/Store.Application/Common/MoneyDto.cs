using Store.Domain.Common;

namespace Store.Application.Common;

/// <summary>An amount and its ISO 4217 currency code.</summary>
public sealed record MoneyDto(decimal Amount, string Currency)
{
    public static MoneyDto From(Money money)
    {
        ArgumentNullException.ThrowIfNull(money);
        return new MoneyDto(money.Amount, money.Currency.Code);
    }
}
