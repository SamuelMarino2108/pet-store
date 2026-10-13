# CLAUDE.md

## Proyecto

Tienda online de accesorios para mascotas (nicho inicial: perros y gatos, modelo dropshipping), construida desde cero. Por ahora es un proyecto de portafolio: no se venden productos reales, los pagos van en sandbox y el proveedor es simulado. Cada decisión técnica debe poder defenderse en una entrevista, y la arquitectura deja abierta la puerta a vender de verdad más adelante.

Antes de tocar el dominio, lee `docs/domain.md`. Las decisiones de arquitectura viven en `docs/adr/`.

## Stack

- **Backend:** ASP.NET Core Web API (.NET 10, C#), EF Core, PostgreSQL 17
- **Frontend:** React + TypeScript (Vite) + Tailwind, React Router, react-hook-form + zod
- **Infra:** Docker Compose, GitHub Actions (CI)
- **Tests:** xUnit, Testcontainers con Postgres real para integración, Vitest + Testing Library
- **API docs:** OpenAPI nativo + Scalar; tipos TypeScript generados con `openapi-typescript`
- **Pagos:** Wompi en modo sandbox (mercado colombiano), detrás de `IPaymentGateway`
- **Proveedor:** detrás de `ISupplierGateway`, con una implementación simulada. Dropi queda como posible integración futura (ver `docs/adr/0001-supplier-integration.md`)
- **Notificaciones:** correo (SMTP; Mailpit en local) y WhatsApp simulado, detrás de `INotificationSender`

## Prioridades (en este orden)

1. **Resolver un problema real de una tienda:** catálogo, carrito, checkout, pedidos y dashboard de ventas por categoría. Nada genérico ni de tutorial.
2. **Código limpio, mantenible y escalable.**
3. **Repositorio muy ordenado y profesional.**

## Arquitectura

Capas con dependencias hacia adentro:

```
src/
  Store.Domain/          # Entidades, value objects, reglas de negocio (sin dependencias)
  Store.Application/     # Casos de uso, DTOs, interfaces, validaciones
  Store.Infrastructure/  # EF Core, pagos, proveedor, servicios externos
  Store.Api/             # Controllers, middleware, configuración
tests/
  Store.Domain.Tests/
  Store.Application.Tests/
  Store.Api.Tests/       # Integración con Testcontainers
web/                     # Frontend React + TypeScript (tienda + panel admin en /admin)
docs/                    # domain.md, ADRs, diagramas
```

Sin MediatR (licencia comercial): servicios de caso de uso o handlers propios.

## Convenciones de código

- Principios SOLID; inyección de dependencias en todo.
- Controllers delgados: la lógica vive en Application/Domain.
- Nunca exponer entidades en la API: usar DTOs.
- Validación de entrada con FluentValidation.
- Manejo de errores centralizado con middleware y `ProblemDetails`.
- Logging estructurado (Serilog). Sin secretos en el repo: variables de entorno y `appsettings` por ambiente.
- Async/await de extremo a extremo con `CancellationToken`.
- Nombres, código, comentarios y commits en **inglés**.
- Frontend: componentes pequeños y tipados, sin `any`, estado de servidor con TanStack Query.
- Paginación y filtros en el catálogo, con índices en la base de datos.
- Health checks (`/health`) y rate limiting en auth y checkout.
- Aviso visible de "tienda de demostración, no ingreses datos personales reales" en el frontend.

## Repositorio

- Commits convencionales (`feat:`, `fix:`, `refactor:`, `docs:`, `test:`, `chore:`).
- Una rama por funcionalidad y PRs pequeños.
- `.gitignore`, `.editorconfig`, `docker-compose.yml` y workflow de CI.
- README profesional: problema que resuelve, arquitectura con diagrama, cómo correrlo, decisiones técnicas y capturas.

## Fases

1. Estructura de la solución, Product, Category y Money, Docker Compose, CI (build + tests), `.editorconfig`, quitar la plantilla WeatherForecast
2. API de catálogo, datos de ejemplo (seed) y frontend de listado y detalle
3. Carrito y checkout como invitado con código de seguimiento (modelo de Order)
4. Pagos con Wompi sandbox y webhooks
5. Autenticación (admin y clientes), panel admin y dashboard de ventas por categoría
6. Notificaciones, proveedor simulado con avance automático de estados, y despliegue de la demo

## Reglas de trabajo

- Antes de escribir código en cada fase, explica en pocas líneas qué vas a hacer y por qué.
- No avances de fase hasta que yo lo confirme.
- Respuestas concisas y directas, sin relleno.
- Si una decisión mía es mala idea, dímelo y propón una alternativa.
- Tras cambios, ejecuta los tests y el build antes de dar la tarea por terminada.
- Comunícate conmigo en español.