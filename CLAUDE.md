# CLAUDE.md

## Proyecto

Tienda online de accesorios para mascotas (nicho inicial: perros y gatos, modelo dropshipping), construida desde cero. Es un proyecto de portafolio: cada decisión técnica debe poder defenderse en una entrevista.

## Stack

- **Backend:** ASP.NET Core Web API (C#), EF Core, PostgreSQL
- **Frontend:** React + TypeScript (Vite) + Tailwind
- **Infra:** Docker Compose, GitHub Actions (CI)
- **Tests:** xUnit (backend), Vitest + Testing Library (frontend)
- **Pagos:** Stripe en modo prueba (o Wompi), siempre detrás de una abstracción (`IPaymentGateway`)

## Prioridades (en este orden)

1. **Resolver un problema real de una tienda:** catálogo, carrito, checkout, pedidos, estado del pedido y panel de administración. Nada genérico ni de tutorial.
2. **Código limpio, mantenible y escalable.**
3. **Repositorio muy ordenado y profesional.**

## Arquitectura

Capas con dependencias hacia adentro:

```
src/
  Store.Domain/          # Entidades, value objects, reglas de negocio (sin dependencias)
  Store.Application/     # Casos de uso, DTOs, interfaces, validaciones
  Store.Infrastructure/  # EF Core, repositorios, pagos, servicios externos
  Store.Api/             # Controllers, middleware, configuración
tests/
  Store.Domain.Tests/
  Store.Application.Tests/
  Store.Api.Tests/
web/                     # Frontend React + TypeScript
docs/                    # Arquitectura, decisiones (ADRs), diagramas
```

## Convenciones de código

- Principios SOLID; inyección de dependencias en todo.
- Controllers delgados: la lógica vive en Application/Domain.
- Nunca exponer entidades en la API: usar DTOs.
- Validación de entrada con FluentValidation.
- Manejo de errores centralizado con middleware y `ProblemDetails`.
- Logging estructurado (Serilog). Sin secretos en el repo: usar variables de entorno y `appsettings` por ambiente.
- Async/await de extremo a extremo con `CancellationToken`.
- Nombres, código, comentarios y commits en **inglés**.
- Frontend: componentes pequeños y tipados, sin `any`, estado de servidor con TanStack Query.

## Repositorio

- Commits convencionales (`feat:`, `fix:`, `refactor:`, `docs:`, `test:`, `chore:`).
- Una rama por funcionalidad y PRs pequeños.
- Incluir `.gitignore`, `.editorconfig`, `docker-compose.yml` y workflow de CI.
- README profesional: problema que resuelve, arquitectura con diagrama, cómo correrlo, decisiones técnicas y capturas.
- Decisiones importantes documentadas como ADRs en `docs/adr/`.

## Fases

1. Estructura de la solución y modelo de datos
2. API de catálogo y frontend de listado y detalle
3. Carrito y checkout
4. Autenticación JWT y pedidos
5. Pagos, panel admin y despliegue

## Reglas de trabajo

- Antes de escribir código en cada fase, explica en pocas líneas qué vas a hacer y por qué.
- No avances de fase hasta que yo lo confirme.
- Respuestas concisas y directas, sin relleno.
- Si una decisión mía es mala idea, dímelo y propón una alternativa.
- Tras cambios, ejecuta los tests y el build antes de dar la tarea por terminada.
- Comunícate conmigo en español.
