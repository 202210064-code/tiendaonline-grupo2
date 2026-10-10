# Memoria de despliegue — TiendaOnline (URPOnline) en AWS

Guía paso a paso para que puedas **replicar el proyecto completo**: clonarlo, ejecutarlo en tu equipo, compilarlo y desplegarlo en AWS con CloudFormation (VPC en dos zonas, ALB, Auto Scaling con EC2, RDS PostgreSQL, ElastiCache Redis y S3).

> ⚠️ **Antes de empezar, lee esto.**
> - Esta es una **arquitectura de laboratorio**, no de producción. Usa HTTP (puerto 80), sin dominio ni certificado HTTPS, sin WAF y con la contraseña de base de datos en un archivo de la instancia.
> - **Desplegarla cuesta dinero mientras exista**: 2 NAT Gateways, 1 ALB, 2 EC2, RDS Multi-AZ y Redis con 2 nodos. Al terminar la práctica, **borra todo** (sección 9).
> - La plantilla y el código se entregan como material de práctica. Es normal tener que ajustar algo (por ejemplo, el nombre de un paquete en la AMI). Para eso está la sección 8, de diagnóstico.

---

## 1. Qué vas a construir

```
Internet
   │  HTTP :80
   ▼
┌──────────────────────── VPC 10.0.0.0/16 (2 zonas de disponibilidad) ────────────────────────┐
│  Subredes públicas  10.0.1.0/24 · 10.0.2.0/24   →  ALB  +  NAT Gateway (uno por zona)        │
│                                    │ :80                                                      │
│  Subredes de app    10.0.11.0/24 · 10.0.12.0/24 →  Auto Scaling Group (mín. 2 EC2)           │
│                                    Nginx :80  →  Kestrel 127.0.0.1:8080 (TiendaOnline.dll)    │
│                                    │ :5432                │ :6379 (TLS)                       │
│  Subredes de datos  10.0.21.0/24 · 10.0.22.0/24 →  RDS PostgreSQL   ElastiCache Redis         │
└───────────────────────────────────────────────────────────────────────────────────────────────┘
         S3: bucket de artefactos (lo creas tú) · bucket de proveedores (lo crea la plantilla)
```

| Pieza | Para qué sirve |
|---|---|
| **ALB** | Recibe el tráfico de Internet y lo reparte entre las EC2. Revisa `/health`. |
| **EC2 + Nginx + Kestrel** | Nginx recibe en el puerto 80 y reenvía a la app .NET, que escucha solo en `127.0.0.1:8080`. |
| **RDS PostgreSQL** | Guarda productos, pedidos y eventos. Es privado: solo lo ven las EC2. |
| **ElastiCache Redis** | Guarda la sesión y el carrito, para que no se pierdan si el ALB te cambia de EC2. |
| **S3** | Guarda el ZIP compilado de la app y los CSV de proveedores. |
| **IAM Role de EC2** | Le da a la instancia permisos de S3 y SSM **sin claves de acceso**. |

Ninguna EC2 tiene SSH abierto. Para entrar a una instancia se usa **SSM Session Manager**.

---

## 2. Requisitos

> 👉 La guía completa de instalación y configuración (VS Code, extensiones, AWS CLI, credenciales y verificación) está en **[CONFIGURACION_VSCODE.md](CONFIGURACION_VSCODE.md)**. Esto es un resumen.

| Herramienta | Versión | Comprobar |
|---|---|---|
| Git | cualquiera reciente | `git --version` |
| .NET SDK | **8** o superior (el proyecto es `net8.0`) | `dotnet --version` |
| Runtime ASP.NET Core 8 | para ejecutar en local | `dotnet --list-runtimes` |
| AWS CLI | v2 | `aws --version` |
| Cuenta AWS | con permisos para crear VPC, EC2, RDS, ElastiCache, IAM, S3 y CloudFormation | — |
| PostgreSQL y Redis locales | solo si quieres ejecutar la app en tu equipo (sección 4) | — |

Instalación en Windows:

```powershell
winget install Git.Git
winget install Microsoft.DotNet.SDK.8
winget install Amazon.AWSCLI
```

> Después de instalar con `winget`, **cierra y vuelve a abrir la terminal** para que se actualice el PATH. Si no, `aws` y `dotnet` aparecerán como "no encontrado".

### Configurar credenciales de AWS

```powershell
aws configure sso      # recomendado si tu organización usa IAM Identity Center
# o bien:
aws configure          # Access Key temporal de un usuario de laboratorio
aws sts get-caller-identity   # debe mostrar tu cuenta y tu usuario o rol
```

> 🔒 **Nunca** pongas Access Keys en el código, en `appsettings.json`, en el YAML ni en un commit. En tu equipo usa el perfil de AWS CLI y en la EC2 se usa el rol IAM.

---

## 3. Clonar el repositorio

El repositorio es **privado**. El profesor te entrega un **token de acceso de solo lectura**; reemplaza `TOKEN` por ese valor:

```powershell
git clone https://TOKEN@github.com/jjjfrancia/tiendaonline.git
cd tiendaonline
```

Para recibir actualizaciones durante el curso, ejecuta `git pull`.

Archivos importantes:

| Ruta | Contenido |
|---|---|
| `Program.cs`, `Controllers/`, `Views/`, `Models/`, `Data/`, `Services/` | La tienda web: ASP.NET Core MVC con EF Core, PostgreSQL y Redis |
| `Worker/` | Programa de consola que importa el catálogo de proveedores desde CSV, local o S3 |
| `URPOnline_ClaudeCode_Deploy/urponline-oneclick.yaml` | **La plantilla de CloudFormation que vas a desplegar** |
| `URPOnline_ClaudeCode_Deploy/DEPLOY.md` | Guía original de despliegue y advertencias |
| `URPOnline_ClaudeCode_Deploy/SPEC.md`, `CLAUDE.md` | Especificación completa (F01–F10) y reglas para trabajar con Claude Code |
| `scripts/` | Ejemplos de Nginx, systemd, PgBouncer y temporizador del Worker |
| `examples/products.csv` | CSV de ejemplo para el Worker |

> El token es **de solo lectura**: puedes clonar y hacer `git pull`, pero no subir cambios. Si quieres guardar tu trabajo en GitHub, crea un repositorio **privado** en tu cuenta y súbelo ahí (`git remote set-url origin <tu-repo>`). **No compartas el token** ni lo publiques.

---

## 4. (Opcional) Ejecutar en tu equipo

Necesitas PostgreSQL y Redis ejecutándose en local. Crea la base de datos y el usuario:

```sql
CREATE USER urponline WITH PASSWORD 'localonly';
CREATE DATABASE urponline OWNER urponline;
```

`appsettings.json` ya apunta a `localhost:5432` y `localhost:6379` con ese usuario. Para la **primera ejecución**, pide que se creen las tablas y los datos de ejemplo:

```powershell
dotnet restore
$env:ASPNETCORE_ENVIRONMENT = "Development"
$env:InitializeDatabase = "true"          # solo la primera vez: crea tablas y datos semilla
dotnet run --urls http://localhost:8080
```

Abre `http://localhost:8080` y `http://localhost:8080/health`; este último debe responder `{"status":"ok"}`. En las siguientes ejecuciones, omite `InitializeDatabase`.

Si no tienes Redis, en modo `Development` la app usa caché en memoria cuando la cadena de Redis está en blanco. En PowerShell, asígnale un **espacio** (una cadena vacía `""` borra la variable y la app volvería a usar `localhost:6379`): `$env:ConnectionStrings__Redis = " "`.

Para probar el Worker con el CSV local:

```powershell
dotnet run --project Worker -- --source local --file examples/products.csv
```

---

## 5. Compilar y empaquetar la aplicación

La plantilla **no compila C#**: descarga desde S3 un ZIP **ya publicado** que tenga `TiendaOnline.dll` en la raíz.

```powershell
dotnet restore
dotnet build -c Release            # si falla, corrige antes de seguir
dotnet publish TiendaOnline.csproj -c Release -o out
Compress-Archive -Path out\* -DestinationPath urponline-publish.zip -Force
```

En Linux o macOS: `(cd out && zip -r ../urponline-publish.zip .)`

Comprueba que la DLL quedó en la raíz del ZIP, no dentro de una carpeta:

```powershell
Add-Type -AssemblyName System.IO.Compression.FileSystem
[IO.Compression.ZipFile]::OpenRead("$PWD\urponline-publish.zip").Entries.FullName | Select-String '^TiendaOnline.dll$'
```

Si no aparece ninguna línea, el ZIP está mal armado y las EC2 no arrancarán.

---

## 6. Subir el artefacto a S3

Elige **una región** y úsala en todos los pasos (en los ejemplos, `us-east-1`). El nombre del bucket debe ser único en todo AWS; añádele tus iniciales o tu código.

```powershell
$REGION = "us-east-1"
$BUCKET = "urponline-artefactos-TUCODIGO"

aws s3 mb "s3://$BUCKET" --region $REGION
aws s3api put-public-access-block --bucket $BUCKET --public-access-block-configuration BlockPublicAcls=true,IgnorePublicAcls=true,BlockPublicPolicy=true,RestrictPublicBuckets=true
aws s3 cp urponline-publish.zip "s3://$BUCKET/releases/urponline-publish.zip" --region $REGION
aws s3 ls "s3://$BUCKET/releases/"
```

---

## 7. Desplegar la infraestructura con CloudFormation

### 7.1 Validar la plantilla

```powershell
aws cloudformation validate-template --template-body file://URPOnline_ClaudeCode_Deploy/urponline-oneclick.yaml --region $REGION
```

Opcional, para encontrar errores antes de crear nada: `pip install cfn-lint` y luego `cfn-lint URPOnline_ClaudeCode_Deploy/urponline-oneclick.yaml`.

### 7.2 Crear la pila

**Opción A: consola web**

1. CloudFormation → **Create stack** → *With new resources* → **Upload a template file** → `urponline-oneclick.yaml`.
2. Nombre de la pila: `URPOnlineLab`.
3. Parámetros:
   - `ArtifactBucket`: tu bucket del paso 6.
   - `ArtifactKey`: `releases/urponline-publish.zip`.
   - `DBPassword`: mínimo 12 caracteres, **solo letras y números**. Los caracteres `;`, `'` o `"` rompen la cadena de conexión.
   - `EnableMultiAZ`: `true` (más caro y más lento de crear) o `false` para ahorrar en el laboratorio.
4. Marca la casilla que reconoce la creación de recursos IAM (`CAPABILITY_IAM`).
5. **Create stack** y espera `CREATE_COMPLETE`. Suele tardar entre 15 y 30 minutos; RDS y Redis son lo más lento.

**Opción B: AWS CLI**

```powershell
aws cloudformation deploy `
  --stack-name URPOnlineLab `
  --template-file URPOnline_ClaudeCode_Deploy/urponline-oneclick.yaml `
  --capabilities CAPABILITY_IAM `
  --region $REGION `
  --parameter-overrides ArtifactBucket=$BUCKET ArtifactKey=releases/urponline-publish.zip DBPassword=TuClaveSegura2026 EnableMultiAZ=false
```

### 7.3 Ver las salidas

```powershell
aws cloudformation describe-stacks --stack-name URPOnlineLab --region $REGION --query "Stacks[0].Outputs" --output table
```

| Salida | Uso |
|---|---|
| `ApplicationHTTP` | URL pública de la tienda |
| `RDSEndpoint` | Host de PostgreSQL (privado) |
| `RedisPrimary` | Host de Redis (privado, TLS) |
| `S3SupplierBucket` | Bucket para los CSV de proveedores |

### 7.4 Verificar que funciona

> `CREATE_COMPLETE` **no** significa que la app funcione. Significa que los recursos existen. Verifica lo siguiente:

1. **EC2 → Target Groups → (el de la pila) → Targets**: las dos instancias deben estar en `healthy`.
2. Abre `http://<ALBDNS>/health` → `{"status":"ok"}`.

### 7.5 Crear las tablas en RDS (primera vez)

La plantilla arranca la app con `InitializeDatabase=false`, así que **la base de datos está vacía**. `/health` responde porque no consulta la base de datos, pero las páginas de la tienda fallarán hasta que existan las tablas. Para crearlas una sola vez desde **una** de las instancias:

1. EC2 → selecciona una instancia → **Connect** → **Session Manager** → *Connect*.
2. En la terminal de la instancia:

```bash
sudo sed -i 's/^InitializeDatabase=false/InitializeDatabase=true/' /etc/urponline.env
sudo systemctl restart urponline
sleep 20
sudo journalctl -u urponline -n 50 --no-pager      # revisa que no haya errores de conexión
sudo sed -i 's/^InitializeDatabase=true/InitializeDatabase=false/' /etc/urponline.env
sudo systemctl restart urponline
```

3. Abre `http://<ALBDNS>/` y la tienda debería mostrar el catálogo.

> En un entorno real esto se hace con **migraciones de EF Core** ejecutadas por un proceso controlado, no con `EnsureCreated`. Está pendiente como mejora en `SPEC.md` (F01).

---

## 8. Diagnóstico

Entra a una instancia con **Session Manager** y revisa:

```bash
sudo tail -n 100 /var/log/cloud-init-output.log   # ¿falló el arranque (UserData)?
sudo systemctl status urponline nginx
sudo journalctl -u urponline -n 100 --no-pager
sudo nginx -t
curl -s http://127.0.0.1:8080/health
```

| Síntoma | Causa probable | Qué revisar |
|---|---|---|
| Targets en `unhealthy` | La app no arrancó | `cloud-init-output.log`; si falló `dnf install aspnetcore-runtime-8.0`, el nombre del paquete cambió en esa AMI |
| `test -f /opt/urponline/TiendaOnline.dll` falla | El ZIP tiene la DLL dentro de una carpeta | Vuelve a armar el ZIP (sección 5) |
| `aws s3 cp` da 403 | Bucket, clave o región equivocados | `ArtifactBucket` y `ArtifactKey` deben coincidir exactamente y el bucket debe estar en la región de la pila |
| Nginx 502 | Kestrel no escucha en `127.0.0.1:8080` | `journalctl -u urponline` |
| Error de PostgreSQL | Tablas inexistentes, contraseña con caracteres especiales o SG | Sección 7.5; `DBPassword` alfanumérica |
| Error de Redis | Falta TLS o el endpoint es incorrecto | La cadena debe llevar `ssl=true`; usa `RedisPrimary` |
| La pila hace *rollback* | Ver la pestaña **Events** de CloudFormation | El primer evento `CREATE_FAILED` indica la causa real |

---

## 9. Borrar todo (¡obligatorio al terminar!)

1. Vacía el bucket de proveedores que creó la pila. Tiene versionado, así que hay que borrar también las versiones; desde la consola: S3 → bucket → **Empty**. Si no lo vacías, la pila no podrá borrarse.
2. Borra la pila:

```powershell
aws cloudformation delete-stack --stack-name URPOnlineLab --region $REGION
aws cloudformation wait stack-delete-complete --stack-name URPOnlineLab --region $REGION
```

3. RDS tiene `DeletionPolicy: Snapshot`: al borrar la pila **queda un snapshot que sigue costando**. Bórralo en RDS → Snapshots.
4. Borra tu bucket de artefactos:

```powershell
aws s3 rb "s3://$BUCKET" --force
```

5. Revisa en **Billing → Bills** al día siguiente que no queden cargos activos.

---

## 10. Qué NO incluye esta versión (para seguir aprendiendo)

La plantilla es la **fase base**. Según `SPEC.md`, faltan estas mejoras:

- HTTPS 443 con certificado ACM y dominio, más **WAF**.
- Contraseña de base de datos en **Secrets Manager** (hoy se escribe en `/etc/urponline.env`).
- **Migraciones EF Core** en lugar de `EnsureCreated`.
- **Data Protection Key Ring compartido**: sin él, las cookies pueden invalidarse al cambiar de EC2.
- **PgBouncer** (puerto 6432) entre la app y RDS; hoy la app conecta directo al 5432.
- Despliegue automático del **Worker** con temporizador systemd (ver `scripts/urponline-worker.*`).
- Pasarela de pago real; el checkout deja los pedidos en `PENDIENTE_PAGO`.
- Autenticación real para `/Analytics` y `/admin/promotions`; hoy se protegen con una clave en la URL, solo para demo.

### Trabajar las mejoras con Claude Code

En la carpeta del proyecto, abre Claude Code y pídele, por ejemplo:

> «Lee `URPOnline_ClaudeCode_Deploy/SPEC.md` y `CLAUDE.md`. Implementa la mejora F02 (Data Protection Key Ring compartido) con pruebas. Ejecuta `dotnet build` y `dotnet test` y muéstrame los resultados reales.»

Trabaja **una mejora a la vez**. Compila y prueba después de cada una.

---

## 11. Checklist del alumno

- [ ] Clonaste el repo y `dotnet build -c Release` compila sin errores
- [ ] `urponline-publish.zip` tiene `TiendaOnline.dll` en la raíz
- [ ] El ZIP está en tu bucket S3 privado, en la misma región que la pila
- [ ] La pila `URPOnlineLab` llegó a `CREATE_COMPLETE`
- [ ] Los dos targets del ALB están en `healthy`
- [ ] `http://<ALBDNS>/health` responde `{"status":"ok"}`
- [ ] Creaste las tablas (7.5) y la tienda muestra el catálogo
- [ ] **Borraste la pila, el snapshot de RDS y los buckets**
