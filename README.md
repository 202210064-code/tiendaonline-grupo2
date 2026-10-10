# URPOnline v2 — tienda C# + analítica + promociones + Worker AWS SDK

## Ejecutar localmente (sin Docker)
- Requiere .NET SDK 8, PostgreSQL y Redis (servicios instalados y ejecutándose).
- Crear base PostgreSQL `urponline` y usuario local.
- Configurar variables: `ConnectionStrings__Postgres`, `ConnectionStrings__Redis`, `InitializeDatabase=true` SOLO para instalación inicial.
- Ejecutar `dotnet restore` y `dotnet run --urls http://localhost:8080`.
- En local HTTPS usar `dotnet dev-certs https --trust` y perfiles HTTPS, o proxy TLS. La cookie `Secure` no viaja por HTTP plano.
- Dashboard: configurar `Dashboard__AccessKey` y abrir `/Analytics?key=...` para pruebas. Esta clave en querystring es SOLO para entorno de demostración; en despliegue real usar autenticación de usuarios/SSO y no URL secrets.
- Promociones: configurar `Admin__AccessKey` y abrir `/admin/promotions?key=...`. Reemplazar por autenticación real en producción.
- Carga proveedores: `dotnet run --project Worker -- --source local --file examples/products.csv`.
- Carga proveedores desde S3: configurar perfil AWS local (`aws configure sso` / `aws configure`) y ejecutar `dotnet run --project Worker -- --source s3 --bucket TU_BUCKET --key suppliers/products.csv`.
- AWS SDK permite usar credenciales IAM locales o rol de EC2; NO poner Access Key en repositorio.
- En EC2, configurar PostgreSQL vía PgBouncer y Redis ElastiCache; Nginx en puerto 80/Kestrel 8080.
- Pago real PayPal/Culqi/Mercado Pago NO implementado; checkout deja PENDIENTE_PAGO. Existe un endpoint de pago DEMO, DESHABILITADO por defecto.
- Los paneles y el modelo de eventos ilustran la analítica y necesitan controles empresariales de privacidad, autenticación y consentimiento antes de producción.
- PgBouncer y Redis deben ser altamente disponibles; template base no demuestra RTO/RPO por sí solo.
- NOTA: Sin acceso a dotnet SDK ni AWS aquí, compilación/despliegue final deben verificarse durante el laboratorio.
