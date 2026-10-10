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
| 7 | **PostgreSQL 16** | Solo para correr la app en local | `winget install PostgreSQL.PostgreSQL.16` | `psql --version` |
| 8 | **Redis** | Opcional en local | Ver la sección 1.1 | `redis-cli ping` → `PONG` |

> ¿Para qué sirve cada uno?
> - **.NET 8 SDK**: compila y ejecuta la tienda. Si tienes una versión más nueva (9 o 10), también compila proyectos `net8.0`, pero para **ejecutarlos** en local necesitas el runtime 8, que viene incluido con el SDK 8.
> - **AWS CLI**: sube el ZIP a S3 y crea la pila de CloudFormation desde la terminal.
> - **Session Manager plugin**: permite entrar a las EC2 privadas desde tu terminal con `aws ssm start-session`. Sin él, puedes entrar igual desde la consola web (EC2 → Connect → Session Manager).
> - **cfn-lint**: revisa la plantilla de CloudFormation antes de desplegarla y te avisa de errores sin gastar dinero.

### 1.1 Redis en Windows (opcional)

Redis no tiene versión oficial para Windows. Elige una de estas opciones:

- **WSL** (Ubuntu): `wsl --install`, y dentro de Ubuntu `sudo apt install redis-server && sudo service redis-server start`.
- **Docker Desktop**: `docker run -d --name redis -p 6379:6379 redis:7`.
- **Sin Redis**: en modo `Development`, la app usa caché en memoria si la cadena de Redis está en blanco. Ver [MEMORIA_DESPLIEGUE.md](MEMORIA_DESPLIEGUE.md), sección 4.

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

El repositorio es **privado**. Usa el **token de solo lectura** que te da el profesor, en lugar de `TOKEN`:

```powershell
git clone https://TOKEN@github.com/jjjfrancia/tiendaonline.git
cd tiendaonline
code .
```

> Con el token puedes clonar y actualizar (`git pull`), pero no subir cambios. **No lo compartas** fuera del curso. Si quieres guardar tus cambios, súbelos a un repositorio privado tuyo (`git remote set-url origin <tu-repo>`).

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

### 3.6 Base de datos local (si instalaste PostgreSQL)

Abre **SQL Shell (psql)** o SQLTools y ejecuta:

```sql
CREATE USER urponline WITH PASSWORD 'localonly';
CREATE DATABASE urponline OWNER urponline;
```

En SQLTools, añade una conexión PostgreSQL con host `localhost`, puerto `5432`, base de datos `urponline`, usuario `urponline` y contraseña `localonly`.

---

## 4. Verificación final

Pega esto en la terminal de VS Code. Cada línea debe mostrar una versión o un dato, no un error:

```powershell
git --version
dotnet --list-sdks
aws --version
aws sts get-caller-identity
cfn-lint --version
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
| PostgreSQL | `brew install postgresql@16` | `sudo apt install postgresql` |
| Redis | `brew install redis` | `sudo apt install redis-server` |
