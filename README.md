# Pet Store

[![CI](https://github.com/SamuelMarino2108/pet-store/actions/workflows/ci.yml/badge.svg)](https://github.com/SamuelMarino2108/pet-store/actions/workflows/ci.yml)

Online store for dog and cat accessories, built from scratch with a dropshipping model and aimed at the Colombian market.

> **Demo store.** This is a portfolio project: nothing is sold for real, payments run in sandbox and the supplier is simulated. Do not enter real personal data.

## The problem

A small dropshipping store has to answer questions that a basic CRUD does not:

- **Is each product profitable?** The store keeps the supplier's cost next to the sale price and calculates the margin, including manual discounts.
- **What did the customer actually pay?** Order lines keep a snapshot of name, cost and price at purchase time, so reports never depend on today's catalog.
- **Where is my order?** Guest customers track orders with an unguessable code. States move from payment to the supplier to delivery.
- **Which categories sell?** A sales dashboard groups revenue, cost and margin by category and product.

The full business rules are in [`docs/domain.md`](docs/domain.md).

## Architecture

Layered architecture with dependencies pointing inwards: the domain has no dependencies, and infrastructure details (database, payment gateway, supplier) sit behind interfaces.

```mermaid
flowchart LR
    Web["web (React + TS)"] -->|HTTP / JSON| Api
    subgraph Backend
        Api["Store.Api<br/>controllers, middleware"] --> Application["Store.Application<br/>use cases, DTOs, interfaces"]
        Api --> Infrastructure
        Infrastructure["Store.Infrastructure<br/>EF Core, Wompi, supplier"] --> Application
        Application --> Domain["Store.Domain<br/>entities, value objects, rules"]
    end
    Infrastructure --> Db[(PostgreSQL)]
```

| Layer | Contents |
| --- | --- |
| `Store.Domain` | `Product`, `Category`, `Money`, `Sku`, `SupplierReference`. Rules are enforced here and invalid states throw `DomainException`. |
| `Store.Application` | Use cases (`ICatalogService`), DTOs, FluentValidation validators and the ports the infrastructure implements (`ICatalogQueries`; payment and supplier ports arrive in later phases). |
| `Store.Infrastructure` | `StoreDbContext`, EF Core mappings, migrations, catalog queries and the sample data seed. |
| `Store.Api` | Thin controllers, `ProblemDetails` error handling, Serilog request logging, `/health`, OpenAPI. |

## Tech stack

- **Backend:** .NET 10, ASP.NET Core, EF Core 10, PostgreSQL 17
- **Frontend:** React, TypeScript, Vite, Tailwind, TanStack Query *(phase 2)*
- **Tests:** xUnit v3, Testcontainers
- **Logging:** Serilog
- **Infra:** Docker Compose, GitHub Actions
- **Payments:** Wompi sandbox *(phase 4)*

## Getting started

### Requirements

- [.NET SDK 10](https://dotnet.microsoft.com/download)
- [Docker](https://www.docker.com/) (for the database and the integration tests)

### Run everything with Docker Compose

```bash
cp .env.example .env   # optional, defaults work for local development
docker compose up --build
```

- API: http://localhost:8080
- Health check: http://localhost:8080/health
- API reference (Scalar): http://localhost:8080/scalar

Migrations are applied when the API starts, and in development an empty database is filled with a sample catalog (6 categories, 30 products).

### Catalog API

| Endpoint | Description |
| --- | --- |
| `GET /api/categories` | Categories with their product count. |
| `GET /api/products` | Search with `category`, `search`, `minPrice`, `maxPrice`, `inStockOnly`, `sort` (`Newest`, `PriceAsc`, `PriceDesc`, `NameAsc`), `page` and `pageSize` (max 48). Prices filter by final price. |
| `GET /api/products/{slug}` | Product detail. |

Invalid filters return `400` with `ValidationProblemDetails`, and an unknown product returns `404`. Cost and margin are never part of the public API. Sample requests are in [`src/Store.Api/Store.Api.http`](src/Store.Api/Store.Api.http).

### Run the API from the IDE or the CLI

```bash
docker compose up -d db
dotnet run --project src/Store.Api
```

The development connection string in `appsettings.Development.json` points to the Compose database. Its credentials are local-only defaults, not secrets.

### Tests

```bash
dotnet test
```

Domain and application tests run without dependencies. API tests start a PostgreSQL container, so Docker must be running.

### Database migrations

```bash
dotnet tool restore
dotnet ef migrations add <Name> -p src/Store.Infrastructure -s src/Store.Api -o Persistence/Migrations
```

## Repository layout

```
src/                 Backend projects (Domain, Application, Infrastructure, Api)
tests/               Unit and integration tests
docs/domain.md       Business rules
docs/adr/            Architecture decision records
.github/workflows/   CI pipeline
```

## Technical decisions

Each important decision is recorded as an ADR:

- [0001 - Supplier integration behind `ISupplierGateway`](docs/adr/0001-supplier-integration.md)
- [0002 - Persistence with EF Core and PostgreSQL](docs/adr/0002-persistence-with-ef-core-and-postgresql.md)
- [0003 - Testing strategy](docs/adr/0003-testing-strategy.md)
- [0004 - Catalog read model](docs/adr/0004-catalog-read-model.md)

## Roadmap

1. ✅ Solution structure, catalog domain model, Docker Compose and CI
2. Catalog API, seed data, product list and detail pages
3. Cart and guest checkout with tracking code
4. Wompi sandbox payments and webhooks
5. Authentication, admin panel and sales dashboard
6. Notifications, simulated supplier and demo deployment
