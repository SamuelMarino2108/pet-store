# 0001. Supplier integration behind `ISupplierGateway`

- **Status:** Accepted
- **Date:** 2026-10-12

## Context

The store follows a dropshipping model: the supplier owns the stock and ships the orders. The project is a portfolio piece with no real sales, so there is no commercial agreement with a supplier and no production credentials. Still, the domain must keep the rules of a real store (cost vs. sale price, stock copied from the supplier, orders forwarded to the supplier and shipping states coming back).

Dropi is the most common dropshipping platform in Colombia and the natural candidate for a real integration.

## Decision

- The domain only knows a `SupplierReference` per product: a supplier code and the product id in that supplier's catalog. It knows nothing about any specific provider.
- Application defines `ISupplierGateway` with the operations a supplier must offer: query stock and cost, send an order, cancel an order and query shipping status.
- The only implementation for now is a **simulated supplier** that advances order states on a schedule (`SentToSupplier → Shipped → Delivered`) so the full flow can be demonstrated.
- Each real integration (Dropi or any other) would live in Infrastructure and map its logistics states to the order states with its own mapping.

## Consequences

- The full order flow can be demonstrated without external accounts.
- Adding a real supplier is a new `ISupplierGateway` implementation plus configuration, with no domain changes.
- The simulated supplier does not show real-world failures (timeouts, partial shipments, stock out after payment). They must be considered before selling for real.
