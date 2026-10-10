# Configuración del entorno — VS Code + .NET + AWS

Todo lo que necesitas **instalar y configurar** en tu equipo antes de trabajar con el proyecto TiendaOnline (URPOnline). Cuando termines esta guía, sigue con [MEMORIA_DESPLIEGUE.md](MEMORIA_DESPLIEGUE.md).

> Los comandos son para **Windows (PowerShell)**. Al final hay equivalentes para macOS y Linux.
> Después de cada instalación con `winget`, **cierra y vuelve a abrir VS Code** para que reconozca los nuevos comandos.

---

## 1. Programas a instalar

| # | Programa | ¿Obligatorio? | Comando (PowerShell) | Comprobar |
|---|---|---|---|---|
| 1 | **Visual Studio Code** | Sí | `winget install Microsoft.VisualStudioCode` | `code --version` |
| 2 | **Git** | Sí | `winget install Git.Git` | `git --version` |
| 3 | **.NET 8 SDK** | Sí | `winget install Microsoft.DotNet.SDK.8` | `dotnet --list-sdks` (debe aparecer `8.x`) |
| 4 | **AWS CLI v2** | Sí | `winget install Amazon.AWSCLI` | `aws --version` |
| 5 | **Session Manager plugin** | Recomendado | `winget install Amazon.SessionManagerPlugin` | `session-manager-plugin` |
| 6 | **Python 3** + **cfn-lint** | Recomendado | `winget install Python.Python.3.12` y luego `pip install cfn-lint` | `cfn-lint --version` |
| 7 | **Docker Desktop** | Sí, para correr la app en local | `winget install Docker.DockerDesktop` | `docker --version` |
| 8 | **PostgreSQL 16 + pgvector** | Sí, en local (va en Docker) | Ver la sección 3.6 | `docker ps` muestra `urp-postgres` |
| 9 | **Redis** | Recomendado en local (va en Docker) | Ver la sección 3.6 | `docker ps` muestra `urp-redis` |
| 10 | **Modelo de embeddings** | Sí, para la búsqueda semántica | `scripts/descargar-modelo.ps1` (sección 3.7) | existe `ml-models/paraphrase-multilingual-MiniLM-L12-v2/model.onnx` |

> ¿Para qué sirve cada uno?
> - **.NET 8 SDK**: compila y ejecuta la tienda. Si tienes una versión más nueva (9 o 10), también compila proyectos `net8.0`, pero para **ejecutarlos** en local necesitas el runtime 8, que viene incluido con el SDK 8.
> - **AWS CLI**: sube el ZIP a S3 y crea la pila de CloudFormation desde la terminal.
> - **Session Manager plugin**: permite entrar a las EC2 privadas desde tu terminal con `aws ssm start-session`. Sin él, puedes entrar igual desde la consola web (EC2 → Connect → Session Manager).
> - **cfn-lint**: revisa la plantilla de CloudFormation antes de desplegarla y te avisa de errores sin gastar dinero.
> - **Docker Desktop**: levanta PostgreSQL (con la extensión **pgvector**) y Redis sin instalarlos en Windows.
> - **Modelo de embeddings**: convierte textos en vectores para que la búsqueda encuentre **productos relacionados** aunque no coincida ninguna palabra.

### 1.1 ¿Por qué Docker y no instalar PostgreSQL directamente?

La búsqueda semántica necesita la extensión **pgvector**. El instalador de PostgreSQL para Windows no la trae y compilarla es complicado. La imagen `pgvector/pgvector:pg16` ya la incluye. Redis tampoco tiene versión oficial para Windows; también va en Docker.

> Docker Desktop pide reiniciar la PC y activar WSL 2 la primera vez. Después de instalarlo, **ábrelo** (icono de la ballena) antes de usar `docker`.

---

## 2. Extensiones de VS Code

Al abrir la carpeta del proyecto, VS Code mostrará el aviso **"Este repositorio recomienda extensiones"**. Pulsa **Instalar todas**. La lista está en `.vscode/extensions.json`.

También puedes instalarlas desde la terminal:

```powershell
code --install-extension ms-dotnettools.csdevkit
code --install-extension amazonwebservices.aws-toolkit-vscode
code --install-extension redhat.vscode-yaml
code --install-extension kddejong.vscode-cfn-lint
code --install-extension mtxr.sqltools
code --install-extension mtxr.sqltools-driver-pg
code --install-extension anthropic.claude-code
```

| Extensión | Para qué |
|---|---|
| **C# Dev Kit** (incluye C#) | Autocompletado, compilación, depuración (F5) y explorador de soluciones |
| **AWS Toolkit** | Ver S3, CloudFormation, EC2 y CloudWatch Logs desde VS Code |
| **YAML** (Red Hat) | Resaltado y validación de la plantilla. `.vscode/settings.json` ya incluye las etiquetas `!Ref`, `!Sub`, `!GetAtt`… para que no salgan marcadas como error |
| **CloudFormation Linter** | Muestra los errores de `cfn-lint` dentro del editor. Requiere `pip install cfn-lint` |
| **SQLTools + driver PostgreSQL** | Consultar la base de datos local |
| **Claude Code** | Asistente para implementar las mejoras de `SPEC.md` |

---

## 3. Configuración

### 3.1 Git

```powershell
git config --global user.name  "Tu Nombre"
git config --global user.email "tu-correo@ejemplo.com"
```

### 3.2 Clonar y abrir el proyecto

El repositorio es público: https://github.com/jjjfrancia/tiendaonline

```powershell
git clone https://github.com/jjjfrancia/tiendaonline.git
cd tiendaonline
code .
```

> Puedes clonar y actualizar (`git pull`), pero no subir cambios al repositorio del curso. Si quieres guardar tu trabajo en GitHub, haz un **Fork** (botón arriba a la derecha en GitHub) y trabaja sobre tu copia.

### 3.3 Credenciales de AWS

Usa **una** de estas dos opciones:

**A) IAM Identity Center / SSO** (recomendado si te dieron un portal de acceso):

```powershell
aws configure sso
# SSO start URL:  la que te dio el profesor
# SSO region:     us-east-1 (o la indicada)
# Elige la cuenta y el rol; como nombre de perfil escribe: default
aws sso login
```

**B) Access Key de un usuario de laboratorio:**

```powershell
aws configure
# AWS Access Key ID:     AKIA...
# AWS Secret Access Key: ****
# Default region name:   us-east-1
# Default output format: json
```

Comprueba que funcionó:

```powershell
aws sts get-caller-identity     # muestra tu Account y tu Arn
aws configure list              # región y perfil activos
```

> 🔒 Las credenciales se guardan en `%USERPROFILE%\.aws\`, **fuera del proyecto**. Nunca las copies en `appsettings.json`, en el YAML, en un `.env` que vayas a subir ni en un commit. Si una Access Key llega a GitHub, **desactívala de inmediato** en IAM.

### 3.4 Conectar el AWS Toolkit de VS Code

1. Barra lateral → icono de **AWS** → **Connect to AWS**.
2. Elige el perfil que configuraste (`default`) y la misma región.
3. Deberías ver S3, CloudFormation, EC2, etc. en el explorador.

### 3.5 Certificado HTTPS de desarrollo (opcional)

```powershell
dotnet dev-certs https --trust
```

Solo es necesario si vas a probar la app en local por HTTPS.

### 3.6 PostgreSQL con pgvector y Redis (Docker)

Con Docker Desktop abierto:

```powershell
docker run -d --name urp-postgres -e POSTGRES_USER=urponline -e POSTGRES_PASSWORD=localonly -e POSTGRES_DB=urponline -p 5432:5432 pgvector/pgvector:pg16
docker run -d --name urp-redis -p 6379:6379 redis:7
```

Otro día, para volver a encenderlos: `docker start urp-postgres urp-redis`.

> Si antes creaste `urp-postgres` con la imagen `postgres:16` (sin pgvector), bórralo y créalo de nuevo con la línea de arriba: `docker rm -f urp-postgres`. La base se vuelve a llenar al arrancar la app con `InitializeDatabase=true`.

En SQLTools, añade una conexión PostgreSQL con host `localhost`, puerto `5432`, base de datos `urponline`, usuario `urponline` y contraseña `localonly`.

### 3.7 Modelo de embeddings (búsqueda semántica)

La tienda usa el modelo **`paraphrase-multilingual-MiniLM-L12-v2`** (entiende español, 384 dimensiones), ejecutado **en tu equipo** con ONNX Runtime: no usa internet ni APIs pagadas. Pesa 118 MB, por eso **no está en el repositorio**; se descarga con un script que verifica su SHA-256:

```powershell
powershell -ExecutionPolicy Bypass -File scripts/descargar-modelo.ps1
```

(macOS/Linux: `bash scripts/descargar-modelo.sh`)

Queda en `ml-models/paraphrase-multilingual-MiniLM-L12-v2/` (`model.onnx` + `sentencepiece.bpe.model`). Cuando arranca la app, verás en la terminal:

```
Modelo de embeddings cargado desde ...
Embeddings actualizados: 6 producto(s)
```

Cómo funciona:
1. Al arrancar (y cada 10 minutos) la app calcula un vector por producto con `nombre + categoría + descripción` y lo guarda en la tabla `ProductEmbeddings` (columna `vector(384)` de pgvector).
2. Al buscar, convierte tu texto en vector y PostgreSQL ordena los productos por **similitud coseno** (`1 - (embedding <=> consulta)`).
3. La página muestra los resultados por texto y, debajo, **Productos relacionados** con su % de similitud. Prueba: `computadora`, `tomar fotos`, `reloj deportivo`.

Ajustes en `appsettings.json` → `Embeddings`: `RelatedLimit` (cuántos mostrar), `MinSimilarity` (umbral, 0.3) y `RefreshMinutes`.

---

## 4. Verificación final

Pega esto en la terminal de VS Code. Cada línea debe mostrar una versión o un dato, no un error:

```powershell
git --version
dotnet --list-sdks
aws --version
aws sts get-caller-identity
cfn-lint --version
docker ps                                   # deben aparecer urp-postgres y urp-redis
Test-Path ml-models/paraphrase-multilingual-MiniLM-L12-v2/model.onnx   # True
dotnet restore; dotnet build -c Release
```

Si `dotnet build` termina con **Build succeeded**, ya puedes seguir con [MEMORIA_DESPLIEGUE.md](MEMORIA_DESPLIEGUE.md).

---

## 5. Problemas frecuentes

| Problema | Solución |
|---|---|
| `aws`, `dotnet` o `git` "no se reconoce como comando" | Cierra y vuelve a abrir VS Code (o la terminal) después de instalar |
| `aws sts get-caller-identity` → *Unable to locate credentials* | Falta `aws configure` o `aws configure sso` |
| `... the SSO session has expired` | `aws sso login` |
| `dotnet run` → *framework 'Microsoft.AspNetCore.App' version 8.0.0 not found* | Instala el SDK 8: `winget install Microsoft.DotNet.SDK.8` |
| La plantilla YAML aparece llena de errores rojos en `!Ref` | Instala la extensión YAML de Red Hat y abre la **carpeta** del proyecto (no el archivo suelto), para que se aplique `.vscode/settings.json` |
| `cfn-lint` no se encuentra después de `pip install` | Reabre la terminal; si sigue igual, usa `python -m cfnlint ...` |
| C# Dev Kit pide iniciar sesión | Es una cuenta Microsoft gratuita (uso educativo/individual) |
| `docker` → *error during connect* / *cannot find the file specified* | Docker Desktop no está abierto: ábrelo y espera a que diga *Engine running* |
| Log: `extension "vector" is not available` | Tu contenedor usa `postgres:16`. Recréalo con `pgvector/pgvector:pg16` (sección 3.6) |
| Log: `No se encontró el modelo de embeddings` y no aparecen *Productos relacionados* | Ejecuta `scripts/descargar-modelo.ps1` y reinicia la app |
| El script del modelo dice *SHA-256 no coincide* | La descarga se cortó: vuelve a ejecutarlo |

---

## Anexo: macOS y Linux

| Programa | macOS (Homebrew) | Ubuntu/Debian |
|---|---|---|
| VS Code | `brew install --cask visual-studio-code` | paquete `.deb` desde code.visualstudio.com |
| Git | `brew install git` | `sudo apt install git` |
| .NET 8 SDK | `brew install --cask dotnet-sdk@8` | `sudo apt install dotnet-sdk-8.0` |
| AWS CLI v2 | `brew install awscli` | instalador oficial: docs.aws.amazon.com/cli |
| Session Manager plugin | `brew install --cask session-manager-plugin` | paquete `.deb` oficial de AWS |
| cfn-lint | `pip3 install cfn-lint` | `pip3 install cfn-lint` |
| Docker | `brew install --cask docker` | Docker Engine: docs.docker.com/engine/install |
| PostgreSQL + pgvector, Redis | mismos `docker run` de la sección 3.6 | mismos `docker run` de la sección 3.6 |
| Modelo de embeddings | `bash scripts/descargar-modelo.sh` | `bash scripts/descargar-modelo.sh` |
