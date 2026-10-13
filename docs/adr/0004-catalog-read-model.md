# 0004. Catalog read model

- **Status:** Accepted
- **Date:** 2026-10-13

## Context

The public catalog is the most frequent read in the store: filter by category, search by name, filter and sort by price, paginate. Customers see the **final price** (after the manual discount), so that is the price they filter and sort by. The domain calculates the final price and never stores it, so that it cannot drift from the price and the discount.

## Decision

- **Read port in Application.** `ICatalogQueries` returns DTOs and is implemented in Infrastructure with EF Core (`AsNoTracking`). The use case (`CatalogService`) validates the criteria with FluentValidation and normalizes slugs and search text before querying. No generic repository: each query states what it needs.
- **Paging in the database, mapping in the domain.** Filtering, sorting, counting and `Skip`/`Take` run in PostgreSQL. Only the requested page is materialized, and DTOs are built from the entities, so the price shown is `Product.FinalPrice`, the same rule the checkout will use.
- **Stored generated column for the final price.** `final_price_amount` is computed by PostgreSQL (`GENERATED ALWAYS AS ... STORED`) and mapped as a shadow property. It exists only so that SQL can filter, sort and index by final price. It mirrors the domain rounding (half away from zero, to the decimal places of the currency), and an integration test checks that both agree.
- **Indexes:** unique on `products.slug` and `categories.slug`, `(category_id, final_price_amount)` for the most common listing, and a trigram GIN index (`pg_trgm`) on `products.name` for case-insensitive search with `ILIKE`. `%` and `_` typed by the user are escaped.
- **Offset pagination** with a total count and a 48-item page limit. The catalog has hundreds of products, not millions, and offset paging allows jumping to page *n*, which a store UI needs. Keyset pagination can replace it if the catalog grows.
- **Slugs** identify products and categories in URLs. They are derived from the name in the domain and are unique.
- **Sample data** is loaded at startup through the domain factories (not `HasData`), only when the catalog is empty and only where `Database:SeedSampleData` is enabled (development and the Compose demo).

## Consequences

- The final price rule exists in two places: the domain (source of truth) and the generated column SQL. The integration test and the CI check for pending model changes keep them aligned; adding a currency changes the generated SQL and requires a migration.
- Renaming a product or category changes its slug, so old URLs stop working. Redirects from old slugs are out of scope for now.
- Search is case-insensitive but accent-sensitive ("raton" does not find "Ratón"). `unaccent` can be added later with an immutable wrapper function and an expression index.
