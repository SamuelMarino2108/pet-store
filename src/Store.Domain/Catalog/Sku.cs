using System.Text.RegularExpressions;
using Store.Domain.Common;

namespace Store.Domain.Catalog;

/// <summary>
/// Stock keeping unit. Stored upper case so lookups are case-insensitive.
/// </summary>
public sealed partial record Sku
{
    public const int MaxLength = 50;

    private Sku(string value) => Value = value;

    public string Value { get; }

    public static Sku Create(string value)
    {
        var normalized = value?.Trim().ToUpperInvariant() ?? string.Empty;

        if (!SkuPattern().IsMatch(normalized))
        {
            throw new DomainException(
                $"SKU must be 3-{MaxLength} characters long and contain only letters, digits and dashes.");
        }

        return new Sku(normalized);
    }

    public override string ToString() => Value;

    [GeneratedRegex("^[A-Z0-9][A-Z0-9-]{2,49}$")]
    private static partial Regex SkuPattern();
}
