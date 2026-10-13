# Reglas de dominio

Documento de referencia del modelo de negocio. Léelo antes de modificar entidades, estados de pedido, pagos, notificaciones o reportes. Las decisiones técnicas con preguntas abiertas están en `docs/adr/`.

**Alcance actual:** proyecto de portafolio, sin ventas reales. El modelo conserva las reglas de una tienda real; lo que cambia es que los pagos van en sandbox, el proveedor es simulado y las notificaciones externas son simuladas.

## Catálogo

- **Product:** sin variantes (sin tallas ni colores). Cada producto es una unidad vendible con su propio SKU.
- **Category:** cada producto pertenece a una categoría. Nicho inicial: perros y gatos.
- **Money:** value object (`decimal` + moneda, COP por defecto). Nunca `double` ni `float`.
- Cada producto guarda el costo (lo que cobra el proveedor) y el precio de venta.
- Cada producto referencia a su proveedor y su ID externo en el proveedor (`SupplierProductId`). El dominio no conoce detalles de ningún proveedor concreto.
- Se incluyen datos de ejemplo (seed) de productos y categorías para poder demostrar la tienda.

## Precios y descuentos

- El precio de venta lo fija el admin a mano.
- Si el proveedor cambia el costo, el sistema marca el producto con una alerta de margen. No cambia el precio solo. (Propuesta, por confirmar.)
- Los descuentos son manuales: el admin elige uno o varios productos y un porcentaje, y los activa o desactiva cuando quiera. No hay cupones.
- El margen se calcula, no se almacena. Margen bruto = precio final − costo del producto. Margen neto = margen bruto − costo de envío − comisión de la pasarela, cuando esos datos estén disponibles.

## Stock

- En un caso real, el stock lo manejaría el proveedor y la tienda guardaría una copia sincronizada con su fecha de última sincronización.
- Al hacer checkout se vuelve a validar la disponibilidad antes de crear el pedido.
- Con el proveedor simulado, el stock se edita desde el panel admin.

## Pedidos

- **Snapshot:** cada línea de pedido guarda nombre, SKU, categoría, costo, precio base, porcentaje de descuento y precio final al momento de la compra. Los reportes usan este snapshot, nunca el catálogo actual.
- **Estados:** `Pending → Paid → SentToSupplier → Shipped → Delivered`, más `Cancelled` y `Refunded`.
- Las transiciones se validan en Domain. Una transición inválida lanza una excepción de dominio.
- **Pending:** se cancela automáticamente a los 30 días (valor configurable). No se reserva stock porque el stock es del proveedor.
- **Cancelación:** el cliente no puede cancelar libremente. Solo se permite mientras el proveedor no haya despachado. En `SentToSupplier`, solo si el proveedor confirma que no despachó. Si el pedido ya estaba pagado, la cancelación dispara el reembolso por la pasarela y termina en `Refunded`.
- **Envío:** tarifa fija configurable. Es una simplificación; en una tienda real el costo vendría del proveedor.
- **Entrega:** el tiempo estimado que se muestra al cliente es configurable (por defecto 2-3 días) y se presenta como estimado.

## Clientes y acceso

- **Checkout como invitado:** nombre, teléfono, correo y dirección de envío en Colombia, pedidos en el momento de comprar.
- **Código de seguimiento:** cada pedido tiene un código único e impredecible (alta entropía, no secuencial). Para consultar un pedido sin cuenta se pide el código más el correo o teléfono con el que se compró. La consulta tiene rate limiting.
- **Cuentas:** dos roles, `Admin` y `Customer`. La cuenta de cliente es opcional. Por ahora hay un solo admin.
- Cómo se vinculan los pedidos hechos como invitado a una cuenta creada después: por definir.
- **Datos de prueba:** la tienda muestra un aviso de demostración y no debe usarse con datos personales reales.

## Pagos

- Pasarela: Wompi en modo sandbox, detrás de `IPaymentGateway`. No hay cobros reales. Se prueban los métodos que permita el sandbox (tarjeta, PSE, Nequi, etc.) y el backend no depende de uno concreto.
- No hay pago contra entrega por ahora.
- Un pedido pasa a `Paid` solo por el webhook de la pasarela, nunca por el redirect del frontend.
- Los webhooks se validan (firma) y son idempotentes: procesar el mismo evento dos veces no cambia nada.

## Proveedor

- Detrás de `ISupplierGateway`, pensada para poder cambiar o sumar proveedores: consultar stock y costo, enviar pedido, cancelar pedido, consultar estado de envío.
- Implementación actual: simulada. Avanza los estados de los pedidos de forma programada (`SentToSupplier → Shipped → Delivered`) para poder demostrar el flujo completo.
- Dropi queda como posible integración futura (ver `docs/adr/0001-supplier-integration.md`). Los estados logísticos de un proveedor real se traducirían a los estados del pedido con un mapeo propio.

## Notificaciones

- Detrás de `INotificationSender`, con implementaciones de correo y WhatsApp.
- Correo: SMTP real a un buzón de pruebas (Mailpit en local), sobre todo en la compra (confirmación).
- WhatsApp: simulado (se registra el mensaje que se enviaría). Una integración real con la API de WhatsApp Business, con plantillas aprobadas, solo tendría sentido si se vende de verdad.
- Se envían a partir de los cambios de estado del pedido.

## Dashboard de ventas

Solo para admin, bajo `/admin`.

- Ventas agrupadas por categoría y, dentro de cada una, por producto.
- Métricas: unidades vendidas, ingresos, costo, margen, devoluciones y reembolsos, y productos sin ventas.
- Ranking de productos más vendidos por categoría para saber qué rinde mejor.
- Filtro por rango de fechas.
- Solo cuentan pedidos en estado `Paid` o posterior; los `Cancelled` y `Refunded` se excluyen de las ventas y se reportan aparte como devoluciones.
- Exportación a Excel (.xlsx).
- Las consultas usan el snapshot de las líneas de pedido y se resuelven con agregaciones en la base de datos, no en memoria.
- El seed incluye pedidos de varias fechas y categorías para que el dashboard se pueda demostrar.

## Si algún día se vende de verdad

Nada de esto bloquea el portafolio, pero habría que resolverlo antes de vender:

- Integración real con el proveedor (ver ADR 0001) y estados logísticos reales.
- Pasarela en producción (cuenta de comercio con Wompi).
- Política de devoluciones, retracto y garantías, y términos de venta. En Colombia el derecho de retracto en compras a distancia puede chocar con la regla de cancelación de arriba; consultarlo con la SIC o un abogado.
- Habeas Data: política de tratamiento de datos personales.
- Forma de facturar (persona natural o empresa) y datos tributarios.
- Costo de envío real para el cliente y proceso de reclamos y novedades.
- WhatsApp Business real.