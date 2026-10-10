# URPOnline — Especificación maestra para Claude Code

**Estado:** contrato de implementación. No considerar funcionalidades completas hasta que compilen, pasen pruebas y estén demostradas. Partir del ZIP `URPOnline_v2_CSharp_AWS_SDK_Proyecto.zip`, ubicado junto a este paquete.

## 1. Misión y caso empresarial
URPOnline vende cursos de Scrum, AWS y programación por Internet a compradores de varios países. La campaña de Cyber Monday y Navidad exige soportar como **objetivos de diseño, no capacidad demostrada**, 20.000 compradores concurrentes y 2.000 peticiones HTTP/s, con pruebas de carga, medición de cuellos de botella y coste. Los proveedores publican actualizaciones de catálogo cada noche por CSV en S3. Los precios/promociones cambian durante el día sin reiniciar instancias. Los compradores conservan su carrito aunque el balanceador cambie de EC2. El sistema registra compras, inventario y métricas comerciales auditables. Acepta integraciones futuras con PayPal, Culqi y Mercado Pago usando sandbox y webhooks, pero jamás simula un pago como confirmado.

Amenazas: bots, DDoS, abuso de descuentos, reenvío de webhooks, inyección SQL, exposición de secretos, intentos de acceso desde Internet a Redis/PostgreSQL, credenciales robadas, agotamiento de conexiones, pérdida de una zona de disponibilidad y caídas durante la campaña. WAF/Shield, mínimo privilegio, TLS, protección de cookies, idempotencia, defensa en profundidad y trazabilidad.

## 2. Restricciones no negociables
- **.NET 8 / C# / ASP.NET Core MVC** (Razor), EF Core y Npgsql, PostgreSQL en RDS, Redis en ElastiCache, EC2 detrás de ALB + Nginx como proxy a Kestrel; IaC CloudFormation. No usar Lambda ni SQS en el diseño obligatorio.
- Dos AZ, app EC2 en subredes privadas, ALB en públicas, Redis y PostgreSQL privados. Puertos: 443 cliente→ALB (con ACM), 80 ALB→Nginx, 8080 Nginx→Kestrel solo loopback, 6379 app→Redis TLS, 6432 app→PgBouncer privado y 5432 PgBouncer→RDS. **En plantilla inicial se permite 5432 directo app→RDS, pero identificarla como fase base, no afirmar que PgBouncer ya existe.**
- AWS SDK for .NET para S3/Secrets Manager/CloudWatch mediante roles IAM sin claves en código. Redis mediante `Microsoft.Extensions.Caching.StackExchangeRedis` (no requiere SDK AWS); PostgreSQL mediante Npgsql (no requiere SDK AWS). Worker C# + systemd timer para carga CSV, sin componentes serverless.
- Dashboard propio de analítica comercial, separado de la observabilidad operativa en CloudWatch.
- Los objetivos RTO/RPO son **metas contractuales**: web 5 min, pedidos/pagos 5/1 min, carrito 10/5 min, catálogo 30/15 min; validar con ejercicios de desastre.

## 3. Estructura de código solicitada
`src/URPOnline.Web` MVC / APIs y vistas; `src/URPOnline.Application` casos de uso, DTO, puertos y validadores; `src/URPOnline.Domain` entidades, invariantes y eventos de dominio; `src/URPOnline.Infrastructure` EF Core, Redis, S3, secretos, métricas; `src/URPOnline.SupplierWorker` CLI/worker C#; `tests/URPOnline.UnitTests`, `tests/URPOnline.IntegrationTests`, `tests/URPOnline.ArchitectureTests`, `tests/URPOnline.E2E`; `infra/`; `scripts/`; `docs/`. La versión previa no tiene esta separación; refactorizar incrementalmente preservando comportamiento.

## 4. Capacidades y criterios de aceptación
### F01 Catálogo
- Listar cursos; buscar por categoría y texto, paginar y ordenar. Precio decimal/moneda, impuestos explícitos, stock consistente. CRUD administrativo autenticado y autorizado. Migraciones EF Core versionadas, no `EnsureCreated()` en producción.
### F02 Carrito y sesión compartida
- Cookie de sesión con identificador opaco, `HttpOnly`, `Secure`, `SameSite=Lax`, TTL inactivo 30 min. Estado del carrito en Redis con clave y TTL, sin PII innecesaria. En dos EC2, recuperar el carrito de la primera sesión en la segunda. Usar Data Protection Key Ring **compartido y persistente** (por ejemplo S3 + protección KMS correctamente configurada), no `SetApplicationName()` por sí solo. Test de expiración, fallo Redis, sesiones concurrentes y migración entre instancias. Implementar estrategia explícita de consentimiento si aplica.
### F03 Checkout, órdenes, stock e idempotencia
- Reserva atómica de stock y transacción; no sobreventa con llamadas concurrentes. Clave de idempotencia por operación, UNIQUE en PostgreSQL. Guardar moneda, total, promociones aplicadas e histórico inmutable de líneas. Estado inicial `PENDIENTE_PAGO`; `PAGADA` solo después de confirmación auténtica del proveedor; nunca confiar solo en retorno del navegador.
### F04 Pagos
- Adaptadores desacoplados para PayPal, Culqi y Mercado Pago; `MockGateway` solo en desarrollo con etiqueta visible. Verificar firmas webhooks, replay, orden temporal, referencia proveedor y secretos. Incluir configuración sandbox e instrucciones de credenciales. Manejo de cobro duplicado y conciliación.
### F05 Promociones y feature flags
- Regla de descuento válida por campaña, país, producto, rango de vigencia, cupón y límites de usos; decidir y validar en backend durante checkout, nunca confiar en el precio del browser. Feature flags con repositorio PostgreSQL + caché Redis y versión; flags activados sin redeploy, auditoría de quién cambió qué, invalidación de caché y rollback. Probar cambios en 2 instancias.
### F06 Proveedores y CSV nocturno
- `S3 GetObject` con AWS SDK; alternativa CSV local para laboratorio. Valida cabeceras, UTF-8, esquema, SKU duplicado, errores por fila y archivo; staging, transacción/upsert y reporte. `systemd timer` invoca worker .NET una vez de noche por instancia **dedicada o con mecanismo de líder** para evitar importaciones simultáneas en todo el ASG. Diseño idempotente por SHA256 de archivo. No Lambda ni SQS.
### F07 Analítica comercial
- Eventos `page_view`, `product_view`, `add_to_cart`, `checkout_started`, `order_created`, `payment_confirmed` con timestamp UTC, correlation ID, campaña, país, fuente; no guardar tarjetas ni secretos ni datos personales innecesarios. Distinción entre eventos observacionales y verdad contable en pedidos confirmados. Dashboard Razor con ventas netas, conversión, abandono, ticket promedio, productos, origen, país y campaña; filtros de fechas; autorización admin; índices y consultas agregadas; prueba de consistencia de resultados con datos semilla.
### F08 Seguridad
- Autenticación admin (ASP.NET Core Identity o proveedor OpenID Connect documentado), RBAC, CSRF antiforgery en formularios, validación entradas, salidas HTML codificadas, headers seguros, HTTPS extremo y auditoría. IAM Role por EC2; Secrets Manager, KMS, CloudTrail, GuardDuty, WAF; VPC Flow Logs; inspección y parcheado. Limitar acceso de CloudWatch/Secrets/S3 por rol y ARN.
### F09 Observabilidad y resiliencia
- Health checks: `/health/live` y `/health/ready` (DB/Redis con timeouts), logs estructurados sin PII, OpenTelemetry traces/metrics con OTLP/ADOT hacia CloudWatch/X-Ray según agente elegido; alarmas ALB 5xx, targets unhealthy, CPU, ASG, RDS DatabaseConnections/FreeStorageSpace, Redis Evictions/CPU, fallos importación; dashboards operacionales. Reintentos con jitter **solo para operaciones idempotentes**, timeouts, circuit breaker. AWS Reachability Analyzer y Session Manager operativos. WAF no es firewall en EC2.
### F10 Despliegue e infraestructura
- CloudFormation parametrizado; VPC 10.0.0.0/16, subredes públicas (10.0.1.0/24, 10.0.2.0/24), app privadas (10.0.11.0/24, 10.0.12.0/24), datos privados (10.0.21.0/24, 10.0.22.0/24), NAT por AZ, ALB, SG restrictivos, ASG mínimo 2, RDS Multi-AZ, Redis con failover y TLS, bucket S3 privado. ACM + DNS para HTTPS producción; parámetro AMI, clase instancia, costos, política de eliminación y tags. CloudFormation crea recursos, el build y subida del artefacto requieren pipeline o paso previo. No prometer un despliegue sin requisitos previos.

## 5. Matriz de pruebas requerida
| Nivel | Casos obligatorios |
|---|---|
| Unitarias | precio, descuentos, TTL, mapeo DTO, validación CSV, conversiones, idempotencia |
| Integración (PostgreSQL y Redis reales de test) | transacciones, concurrencia/stock, flags distribuidas, recuperación carrito, expiración, migraciones |
| Contract/API | errores HTTP, DTO, webhooks falsificados/duplicados, auth/RBAC, CSRF |
| E2E (Playwright) | catálogo→carrito→checkout→estado pedido→dashboard, admin promociones |
| Rendimiento (k6) | carga progresiva y 2.000 rps objetivo, p95, errores, saturación DB/Redis, costes |
| Resiliencia | terminación EC2, pérdida AZ simulada, pérdida Redis, failover RDS, restauración PITR |
| Infraestructura | lint CloudFormation, cfn-lint, seguridad SG, sin puertos 5432/6379 públicos, TLS |
| Seguridad | SAST, secret scan, dependencias, OWASP, webhook replay, carga maliciosa CSV |

**Definition of Done:** compila sin advertencias críticas, migraciones reproducibles, pruebas automatizadas pasan con evidencias, despliegue ambiente de prueba verificable, documentación de variables, costes, teardown y rollback. No falsear pruebas ni métricas.

## 6. Plan de Claude Code por entregables verificables
1. Inventariar ZIP, compilar con `dotnet restore`, `dotnet build`, `dotnet test`, anotar fallos reales (no inventar salida).
2. Estabilizar arquitectura y migraciones; agregar pruebas de humo.
3. Resolver sesión Redis distribuida y protección de claves; pruebas dos instancias.
4. Implementar comercio transaccional, pagos mock + adaptadores, feature flags y Worker S3.
5. Implementar analítica y dashboard; pruebas E2E.
6. Endurecer IaC/bootstrapping, IAM, telemetría, SG y HTTPS; ejecutar `cfn-lint`.
7. Ejecutar pruebas integración/carga/seguridad; entregar matriz de trazabilidad `Fxx → código → prueba → resultado`.
8. Publicar guía para despliegue local y AWS, smoke tests, runbooks RTO/RPO y un changelog.

## 7. Prompt inicial recomendado
"Lee íntegramente `SPEC.md` y `CLAUDE.md`. Audita el proyecto URPOnline ZIP y crea una matriz requisito F01–F10/estado/evidencia. Empieza por compilar, corregir fallos y escribir tests base. Implementa por fases, nunca declares completado sin prueba; mantén arquitectura EC2/Nginx/Kestrel/Redis/PgBouncer/RDS y respeta la prohibición de Lambda/SQS. Tras cada fase reporta archivos cambiados, comandos ejecutados, resultados y riesgos pendientes."
