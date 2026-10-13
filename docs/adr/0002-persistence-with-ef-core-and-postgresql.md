# 0002. Persistence with EF Core and PostgreSQL

- **Status:** Accepted
- **Date:** 2026-10-12

## Context

The domain uses value objects (`Money`, `Sku`, `SupplierReference`) and entities with private setters. Persistence must not force the domain to expose public setters or to depend on EF Core. The dashboard will run aggregations over order lines, so a relational database with good support for grouping and numeric types is needed.

## Decision

- **PostgreSQL 17** with **EF Core 10** (Npgsql provider). Mapping lives in `Store.Infrastructure/Persistence/Configurations`, one `IEntityTypeConfiguration` per entity. The domain has no EF attributes.
- **Value objects as complex types** (`ComplexProperty`): `Money` becomes two columns on the owning table (`price_amount`, `price_currency`). There are no extra tables or joins, and two products with the same price do not share a row.
- **Single-value objects** (`Sku`) use value converters.
- **`numeric(18,2)`** for amounts. It holds COP and USD without floating point errors.
- **snake_case names** (`EFCore.NamingConventions`), the PostgreSQL convention, so manual SQL does not need quoted identifiers.
- **UUID v7 ids generated in the domain** (`Guid.CreateVersion7()`). An entity has its id before it is saved, and v7 ids are time ordered, so the primary key index does not fragment like it would with random UUIDs.
- **Migrations in the repository**, generated with the local `dotnet-ef` tool. The API applies them on startup only when `Database:ApplyMigrationsOnStartup` is `true` (development, Docker Compose and tests). A real production deployment would run them as a separate step, for example with a migration bundle.
- CI fails if the model changed without a migration (`dotnet ef migrations has-pending-model-changes`).

## Consequences

- The domain stays persistence-ignorant. It only has a private parameterless constructor on `Product` so EF can materialize it.
- Calculated values (final price, margin) are not stored, so they cannot go out of sync.
- Adding a new value object requires its mapping in the configuration classes.
- Still open: whether Application uses the `DbContext` through an interface (`IAppDbContext`) or through repositories per aggregate. It will be decided with the first use case (phase 2).
