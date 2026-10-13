namespace Store.Domain.Common;

/// <summary>
/// ISO 4217 currency supported by the store, with the number of decimal places it allows.
/// </summary>
public sealed record Currency
{
    public static readonly Currency Cop = new("COP", 0);
    public static readonly Currency Usd = new("USD", 2);

    private static readonly Dictionary<string, Currency> Supported = new(StringComparer.OrdinalIgnoreCase)
    {
        [Cop.Code] = Cop,
        [Usd.Code] = Usd,
    };

    private Currency(string code, int decimalPlaces)
    {
        Code = code;
        DecimalPlaces = decimalPlaces;
    }

    public string Code { get; }

    public int DecimalPlaces { get; }

    public static IReadOnlyCollection<Currency> All => Supported.Values;

    public static Currency FromCode(string code) =>
        Supported.TryGetValue(code?.Trim() ?? string.Empty, out var currency)
            ? currency
            : throw new DomainException($"Currency '{code}' is not supported.");

    public override string ToString() => Code;
}
