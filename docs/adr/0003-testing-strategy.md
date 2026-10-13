# 0003. Testing strategy

- **Status:** Accepted
- **Date:** 2026-10-12

## Context

Business rules live in the domain and must be tested quickly and in isolation. Persistence and the HTTP pipeline must be tested against the real database engine, because an in-memory provider would hide errors in mappings, constraints and SQL translation.

## Decision

- **xUnit v3** on **Microsoft.Testing.Platform**, enabled in `global.json`. On .NET 10, `dotnet test` requires this runner for xUnit v3.
- **Plain `Assert`** from xUnit. FluentAssertions now has a commercial license, and plain asserts are enough here.
- **`Store.Domain.Tests`:** unit tests, no I/O. One test class per domain type.
- **`Store.Api.Tests`:** integration tests with `WebApplicationFactory` and **Testcontainers** (`postgres:17-alpine`). An assembly fixture starts one container for the whole run and applies migrations on startup. Tests create their own data with unique names, so they do not depend on each other.
- `Store.Application.Tests` will be added with the first use case.
- CI runs every test on each push and pull request. GitHub runners have Docker available.

## Consequences

- Running the integration tests locally requires Docker.
- The first integration run downloads the PostgreSQL image. After that the container starts in a few seconds.
- Mapping errors and missing migrations are caught by the tests, not in production.
