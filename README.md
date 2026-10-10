# URPOnline — Laboratorio AWS (TiendaOnline .NET)

## 👉 Empieza aquí

1. **Prepara tu equipo:** [CONFIGURACION_VSCODE.md](CONFIGURACION_VSCODE.md) — qué instalar y configurar (VS Code, .NET 8, AWS CLI, extensiones).
2. **Sigue la ruta del curso en orden:** carpeta [`RUTA_DEL_CURSO/`](RUTA_DEL_CURSO/) — abre primero `URPOnline_INICIO_AQUI.docx`.

| Paso | Carpeta | Contenido |
|---|---|---|
| 01 | `RUTA_DEL_CURSO/01_CASO_Y_ARQUITECTURA` | Caso empresarial, arquitectura y dashboard de analítica |
| 02 | `RUTA_DEL_CURSO/02_APLICACION` | La aplicación C# (código en la raíz de este repo) |
| 03 | `RUTA_DEL_CURSO/03_INSTALACION_Y_AWS_SDK` | Configuración local y AWS SDK (100 preguntas) |
| 04 | `RUTA_DEL_CURSO/04_SESIONES_REDIS` | Sesiones con Redis |
| 05 | `RUTA_DEL_CURSO/05_CLAUDE_CODE_Y_DESPLIEGUE` | Claude Code + CloudFormation → [MEMORIA_DESPLIEGUE.md](MEMORIA_DESPLIEGUE.md) |
| 06 | `RUTA_DEL_CURSO/06_DESAFIOS_AVANZADOS` | Ingeniería distribuida y pruebas de caos |

---

## 🔑 Conectar tu equipo a tu cuenta de AWS (AWS CLI)

**0. Descarga e instala la AWS CLI v2** (una sola vez):

- **Windows** — en PowerShell: `winget install Amazon.AWSCLI`
  o descarga el instalador: https://awscli.amazonaws.com/AWSCLIV2.msi (siguiente → siguiente → finalizar)
- **macOS** — https://awscli.amazonaws.com/AWSCLIV2.pkg o `brew install awscli`
- **Linux** — guía oficial: https://docs.aws.amazon.com/cli/latest/userguide/getting-started-install.html

Comprueba la instalación (después de **cerrar y volver a abrir VS Code**): `aws --version` → debe mostrar `aws-cli/2.x.x`.

> El **AWS SDK for .NET** (S3, Secrets Manager, CloudWatch) **no se instala aparte**: ya está en el proyecto y lo descarga `dotnet restore`. Usa las mismas credenciales que configuras aquí con `aws configure`.

**1. Abre la terminal de VS Code:** menú **Terminal → New Terminal**.

> Si `aws` dice *"no se reconoce como comando"*: cierra **todas** las ventanas de VS Code y vuelve a abrirlo (recién instalado no se ve). Mientras tanto puedes usar la ruta completa: `& "C:\Program Files\Amazon\AWSCLIV2\aws.exe" configure`

**2. Crea tu Access Key** en la consola de AWS:
1. Arriba a la derecha haz clic en **tu nombre** → **Security credentials**.
2. Sección **Access keys** → **Create access key** (si avisa que no se recomienda en la cuenta root, marca la casilla y continúa: es para el laboratorio).
3. Copia los dos valores — el secreto **solo se muestra una vez**, descarga el `.csv`:
   - **Access key ID** → 20 caracteres, **empieza con `AKIA`**
   - **Secret access key** → 40 caracteres

> ⚠️ El número de 12 dígitos de la página *Account* (ej. `5013-8083-3144`) es el **ID de cuenta**, **no** es la clave. Tampoco lo son los códigos de MFA.

**3. Configura la CLI** en la terminal:

```powershell
aws configure
```

| Te pregunta | Escribe |
|---|---|
| `AWS Access Key ID` | la clave que empieza con `AKIA` |
| `AWS Secret Access Key` | el secreto |
| `Default region name` | el **código** de la región, no el nombre: Ohio = **`us-east-2`**, N. Virginia = **`us-east-1`** |
| `Default output format` | **`json`** (es el formato en que se muestran los resultados) |

**4. Verifica:**

```powershell
aws sts get-caller-identity
```

Debe mostrar tu `Account` (12 dígitos) y tu `Arn`. 🔒 **Nunca pegues la Access Key ni el secreto en chats, código, correos ni en GitHub.**

| Error | Causa | Solución |
|---|---|---|
| `Could not connect to the endpoint URL: https://sts.OHIO.amazonaws.com` | Pusiste el nombre de la región | `aws configure set region us-east-2` |
| `InvalidClientTokenId` / `The security token included in the request is invalid` | La Access Key está mal copiada o no es una Access Key | Crea una nueva (paso 2) y vuelve a `aws configure` |
| `Unable to locate credentials` | No ejecutaste `aws configure` | Paso 3 |
| `SignatureDoesNotMatch` | El secreto está mal copiado | Vuelve a `aws configure` y pega el secreto completo |

---

## 🚀 Descargar y correr la tienda en tu equipo

Requisitos: **.NET 8 SDK**, **Docker Desktop** abierto y **Git**. Cómo instalarlos: [CONFIGURACION_VSCODE.md](CONFIGURACION_VSCODE.md).

```powershell
# 1. Descargar el proyecto
git clone https://github.com/jjjfrancia/tiendaonline.git
cd tiendaonline

# 2. PostgreSQL con pgvector + Redis (Docker)
docker run -d --name urp-postgres -e POSTGRES_USER=urponline -e POSTGRES_PASSWORD=localonly -e POSTGRES_DB=urponline -p 5432:5432 pgvector/pgvector:pg16
docker run -d --name urp-redis -p 6379:6379 redis:7

# 3. Modelo de búsqueda semántica (118 MB, se descarga una vez)
powershell -ExecutionPolicy Bypass -File scripts/descargar-modelo.ps1

# 4. Arrancar la tienda (la primera vez crea las tablas y los productos de ejemplo)
$env:ASPNETCORE_ENVIRONMENT = "Development"
$env:InitializeDatabase = "true"
$env:Dashboard__AccessKey = "demo-dashboard"
$env:Admin__AccessKey = "demo-admin"
dotnet run --urls http://localhost:8080
```

5. Abre **http://localhost:8080**

| Qué probar | Dónde |
|---|---|
| Tienda, carrito y compra | http://localhost:8080 |
| Búsqueda semántica (pgvector): busca `tomar fotos` o `reloj deportivo` → **Productos relacionados** | barra de búsqueda |
| Analítica | http://localhost:8080/Analytics?key=demo-dashboard |
| Promociones | http://localhost:8080/admin/promotions?key=demo-admin |

Otro día: `docker start urp-postgres urp-redis` y `dotnet run --urls http://localhost:8080` (ya sin `InitializeDatabase`).
¿Algo falla? Tabla de **Problemas frecuentes** en [CONFIGURACION_VSCODE.md](CONFIGURACION_VSCODE.md#5-problemas-frecuentes).

---

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
