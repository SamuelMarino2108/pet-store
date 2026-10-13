using Microsoft.Extensions.Logging;

namespace Store.Infrastructure.Persistence.Seeding;

internal static partial class SeedingLog
{
    [LoggerMessage(Level = LogLevel.Information, Message = "Seeded sample catalog with {ProductCount} products")]
    public static partial void CatalogSeeded(ILogger logger, int productCount);
}
