# CLAUDE.md — Reglas operativas obligatorias para Claude Code / URPOnline

## Fuente de verdad y autoridad
- Leer **primero `SPEC.md`**, después este `CLAUDE.md`, luego `README.md`, `.csproj`, `Program.cs` y `infra/*.yaml`.
- La implementación debe demostrar cada criterio de aceptación de `SPEC.md`; no degradar requisitos sin documentar un ADR y solicitar aprobación.
- No generar código ficticio, mocks silenciosos en producción, pruebas falsas, capturas inventadas o servicios AWS declarados pero no usados.

## Arquitectura y diseño
- C# .NET 8, ASP.NET Core MVC + Razor, código asíncrono con `CancellationToken` en I/O. Capas Domain → Application → Infrastructure/Web; dependencias apuntan hacia adentro, infraestructura implementa interfaces de Application.
- Domain sin dependencia de EF Core, AWS SDK, HTTP, Redis ni UI. Modelar invariantes y políticas en dominio; no controladores con lógica de negocio.
- DTO separados de entidades; validaciones de entrada; servicios DI scoped/transient/singleton correctamente; errores tipados, `ProblemDetails` para API y vista amigable para MVC.
- PostgreSQL como fuente de verdad de pedidos, pagos, inventario; Redis solo estado efímero/sesión/caché. No usar Redis como contabilidad ni asumir transacciones distribuidas.
- Feature flags dinámicas con versión, auditoría, cache invalidation y fallback explícito. No usar toggles frontend como control de autorización.
- Proveedor de pagos real mediante adaptador y sandbox; mock **solamente** si `Environment.IsDevelopment()` y la UI señala simulación.
- PgBouncer en puerto 6432, RDS 5432. No conectar a 6432 si el componente aún no fue desplegado. Documentar topología base directa 5432 frente a topología avanzada 6432.
- Nginx en 80 privado desde ALB, Kestrel en 127.0.0.1:8080, Redis 6379 TLS; 443 público requiere ALB HTTPS+ACM. **Nunca habilitar puertos 5432, 6379 ni 8080 a Internet.**

## Calidad del código
- `Nullable=enable`, `TreatWarningsAsErrors` en CI cuando las dependencias lo permitan, `dotnet format --verify-no-changes`, analyzers, convenciones nombres, límites de clase y SRP, evitar `catch(Exception)` que silencie errores.
- Migraciones EF Core versionadas: **no `EnsureCreated()` en producción**. Seed de datos solo en desarrollo o tarea controlada.
- Usar UTC, `decimal` para dinero, moneda ISO 4217, timestamps y culture explícitos; no `float` para importes.
- No duplicar implementaciones Redis/AWS; encapsular SDK (`IAmazonS3`, SecretsManager, CloudWatch) tras interfaces de infraestructura. No hardcodear región/secretos; parametrizar.
- Evitar dependencias cíclicas y acoplamiento controlador↔AWS. Documentar ADR para decisiones complejas.

## Seguridad
- **Nunca** credenciales AWS AccessKey/SecretKey en repositorio, YAML, docker image, logs o ejemplos reales. Local: AWS profile/SSO; EC2: Instance Profile IAM Role. IAM mínimo privilegio.
- Secretos en Secrets Manager y variables solo en entornos de laboratorio; protección de datos de ASP.NET Core con Key Ring compartido y cifrado; cookies Secure+HttpOnly+SameSite, TLS; protección CSRF.
- Distinguir autenticación de autorización. Endpoints `/admin`, `/analytics`, flags y promociones requieren políticas/roles. No exponer panel comercial públicamente.
- Inyección SQL: consultas parametrizadas/EF Core; seguridad de cargas CSV; límites tamaño; antifraude e idempotencia.
- Webbhooks de pagos: firma y tiempo de recepción validados, deduplicación, no marcar pagado por callback navegador.
- Logs/trazas libres de tokens, secretos, contenido de carrito sensible, identificadores de sesión y datos de tarjeta.
- Configurar `ForwardedHeaders` solo para proxies confiables, no confiar automáticamente en toda IP.

## Pruebas y evidencia
- Antes de modificar: ejecutar y registrar `dotnet --info`, `dotnet restore`, `dotnet build`, `dotnet test`. Reportar bloqueos (SDK faltante, red, AWS sin credenciales) sin inventar éxito.
- Unit tests: xUnit + FluentAssertions o equivalente aprobado. Integration tests: Postgres y Redis efímeros vía Testcontainers (si Docker disponible). E2E: Playwright. Carga: k6.
- TDD en requisitos críticos: stock concurrente, idempotencia de checkout/pagos, flags, sesión redis entre dos nodos, importación CSV repetida, KPI conversiones.
- Por fase: pruebas añadidas, resultado real, limitaciones, regresión. Prohibido afirmar “producción” si no se validaron TLS, IAM, SG, rendimiento, recuperación y backup.
- Infra: `cfn-lint`, `aws cloudformation validate-template`, comprobación de cambios antes de ejecutar; validar `TargetGroup` y `/health`.

## AWS y despliegue
- `infra/urponline-oneclick.yaml` es una plantilla parametrizada; `ArtifactBucket` y `ArtifactKey` deben referirse a un ZIP de **binarios compilados** con `URPOnline.dll` en la raíz, no al ZIP de fuentes.
- El `UserData` instala .NET runtime y Nginx, descarga desde S3, configura env/systemd, comprueba servicio. Una pila CREATED no significa sistema funcional: verificar ALB targets healthy, HTTP/HTTPS, RDS, Redis, dashboard y logs.
- Costo: 2 NAT gateways, ALB, 2 EC2, Redis 2 nodos, RDS generan cargos. Documentar borrado de stack, snapshots y buckets.
- La plantilla **base** no incluye aún PgBouncer, ACM/WAF, entorno de pagos, Redis Data Protection Key Ring ni auto-deploy del Worker. Señalar esta brecha y crear incremento separado antes de afirmar paridad total F01–F10.

## Proceso de trabajo de Claude Code
1. Leer y mapear requerimiento→archivo→prueba. Elegir cambios pequeños, revisables.
2. Crear rama/commit con alcance; no borrar trabajos existentes sin aprobación.
3. Implementar código + pruebas + configuración + documentación y ejecutar comandos reales.
4. Producir resumen de diferencias, riesgos de seguridad, costes AWS, comandos exactos de verificación, evidencia de tests.
5. Ante ambigüedad de un requisito con impacto monetario/seguridad, formular pregunta y no inventar políticas.

## Comandos estándar
```bash
dotnet restore
dotnet build -c Release
dotnet test -c Release
# cuando estén instalados:
dotnet format --verify-no-changes
cfn-lint infra/urponline-oneclick.yaml
aws cloudformation validate-template --template-body file://infra/urponline-oneclick.yaml
```
