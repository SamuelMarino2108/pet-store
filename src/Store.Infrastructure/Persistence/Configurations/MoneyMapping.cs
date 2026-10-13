using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Store.Domain.Common;

namespace Store.Infrastructure.Persistence.Configurations;

/// <summary>
/// Maps <see cref="Money"/> as a complex type: two columns on the owning table
/// (for example <c>cost_amount</c> and <c>cost_currency</c>) and no separate table.
/// </summary>
internal static class MoneyMapping
{
    public static void Configure(ComplexPropertyBuilder<Money> money)
    {
        money.Property(m => m.Amount)
            .HasPrecision(18, 2);

        money.Property(m => m.Currency)
            .HasConversion(currency => currency.Code, code => Currency.FromCode(code))
            .HasMaxLength(3);
    }
}
