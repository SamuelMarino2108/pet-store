using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Store.Application.Catalog;

namespace Store.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly, includeInternalTypes: true);
        services.AddScoped<ICatalogService, CatalogService>();

        return services;
    }
}
