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

## ☁️ Desplegar la tienda en AWS

> 📄 **Archivos de CloudFormation (carpeta `infra/`):**
> - [`infra/urponline-oneclick.yaml`](infra/urponline-oneclick.yaml) → **la tienda completa en AWS** (este es el que despliegas).
> - [`infra/github-oidc.yaml`](infra/github-oidc.yaml) → conexión GitHub Actions → AWS sin Access Keys (solo para la opción B).
>
> Si ves otra copia de `urponline-oneclick.yaml` en otra carpeta o en un ZIP del curso, **no la uses**: es una versión anterior.

Toda la arquitectura está escrita en ese archivo **YAML de CloudFormation**. AWS lee el archivo y crea **todo**:

| Capa | Recursos que crea el YAML |
|---|---|
| Red | VPC `10.0.0.0/16`, subredes **públicas** (ALB, NAT), **de aplicación** (EC2) y **de datos** (RDS, Redis) en 2 zonas, IGW, 2 NAT, tablas de rutas, security groups |
| Entrada | **CloudFront** con **HTTPS** (certificado de AWS, sin dominio propio) → **ALB** público en el puerto 80 |
| Aplicación | **Launch Template + Auto Scaling** (2 EC2 privadas con Nginx → Kestrel) |
| Datos | **RDS PostgreSQL 16** (con pgvector), **ElastiCache Redis** (sesiones, TLS), **S3** de proveedores |
| Seguridad | **Rol IAM** para las EC2 (S3 + Session Manager, sin claves) |

### 🔒 HTTPS: certificado digital

Para que la tienda abra con candado (`https://`) hace falta un **certificado digital**. En AWS hay dos caminos:

| | Opción 1 — CloudFront (**ya incluida en la plantilla**) | Opción 2 — Dominio propio + ACM |
|---|---|---|
| ¿Necesita dominio? | **No** | Sí (ej. `urponline.com`, ~13 USD/año en Route 53) |
| Certificado | El de AWS para `*.cloudfront.net` (automático) | **AWS Certificate Manager (ACM)**, gratis, para tu dominio |
| URL | `https://xxxx.cloudfront.net` | `https://tienda.tudominio.com` |
| Servicios | CloudFront → ALB | Route 53 + ACM + listener 443 en el ALB (y opcional CloudFront) |

> ¿Por qué no un certificado ACM para la dirección del ALB? ACM solo emite certificados para dominios que **demuestras que son tuyos**; `...elb.amazonaws.com` es de AWS. Por eso, **sin dominio**, el camino es CloudFront.

#### Opción 1 — CloudFront (sin dominio)

Ya está en `infra/urponline-oneclick.yaml` (recurso **`CDN`**): no hay que hacer nada extra.

- La URL segura aparece en **CloudFormation → URPOnlineLab → Outputs → `ApplicationHTTPS`**.
- Si entras por `http://` CloudFront te redirige a `https://`.
- Las páginas **no se guardan en caché** (el carrito y los precios siempre al día); los `/css/*` y `/js/*` **sí** (cargan más rápido).
- CloudFront tarda **5-10 min** en publicarse después de crearse; si al principio da error, espera y recarga.
- Compruébalo: abre la URL y haz clic en el **candado** del navegador → *La conexión es segura* → certificado emitido por **Amazon** para `*.cloudfront.net`.

#### Opción 2 — Con dominio propio (ACM + Route 53)

1. **Dominio en Route 53:** Consola → **Route 53 → Registered domains → Register domain** (o, si lo compraste en otro proveedor, **Hosted zones → Create hosted zone** y copia los 4 servidores *NS* en tu proveedor).
2. **Certificado en ACM** (en la **misma región del ALB**, ej. Ohio): Consola → **Certificate Manager → Request → Request a public certificate** → *Fully qualified domain name*: `tienda.tudominio.com` → **DNS validation** → **Request**. Abre el certificado → **Create records in Route 53** → espera el estado **Issued** (≈5 min).
3. **Listener HTTPS en el ALB:** Consola → **EC2 → Load Balancers** → el ALB de `URPOnlineLab` → **Listeners → Add listener** → *HTTPS : 443* → *Forward to* el target group de la pila → *Certificate*: el de ACM → **Add**.
4. **Abrir el puerto 443:** **EC2 → Security Groups** → el del ALB (`URPOnline ALB public HTTP...`) → **Edit inbound rules** → *Add rule*: HTTPS, 443, `0.0.0.0/0`.
5. **Registro DNS:** **Route 53 → Hosted zones → tu dominio → Create record** → nombre `tienda` → *Alias* → *Alias to Application Load Balancer* → región Ohio → el ALB → **Create**.
6. Abre `https://tienda.tudominio.com`.

> Los pasos 3 y 4 hechos a mano en la consola se pierden si borras la pila. Para dejarlos fijos, agrégalos al YAML como un `AWS::ElasticLoadBalancingV2::Listener` de puerto 443 con `Certificates: [{CertificateArn: ...}]` y una regla 443 en `AlbSG`.

### ⚡ Opción rápida: AWS CloudShell (sin instalar nada, sin Access Keys)

**CloudShell** es una terminal dentro de la consola de AWS que ya trae la AWS CLI conectada a tu cuenta. El ZIP de la tienda ya está compilado en [Releases v1.0](https://github.com/jjjfrancia/tiendaonline/releases/tag/v1.0).

1. Consola de AWS → arriba a la derecha elige la región **Ohio (us-east-2)**.
2. Haz clic en el icono **`>_`** de la barra superior (o busca **CloudShell**).
3. Pega este bloque y presiona **Enter**:

```bash
cd ~ && rm -rf tiendaonline && git clone https://github.com/jjjfrancia/tiendaonline.git && cd tiendaonline
REGION=us-east-2; STACK=URPOnlineLab
ACCOUNT=$(aws sts get-caller-identity --query Account --output text)
BUCKET=urponline-artefactos-$ACCOUNT-$REGION
DBPASS=$(tr -dc A-Za-z0-9 </dev/urandom | head -c 20); echo "$DBPASS" > ~/urponline-dbpass.txt
curl -fL -o urponline-publish.zip https://github.com/jjjfrancia/tiendaonline/releases/download/v1.0/urponline-publish.zip
aws s3 mb s3://$BUCKET --region $REGION; aws s3 cp urponline-publish.zip s3://$BUCKET/releases/urponline-v1.zip
aws cloudformation deploy --stack-name $STACK --region $REGION --template-file infra/urponline-oneclick.yaml --capabilities CAPABILITY_IAM --parameter-overrides ArtifactBucket=$BUCKET ArtifactKey=releases/urponline-v1.zip DBPassword=$DBPASS EnableMultiAZ=false
aws cloudformation describe-stacks --stack-name $STACK --region $REGION --query "Stacks[0].Outputs" --output table
```

| Línea | Qué hace |
|---|---|
| `git clone ...` | Descarga el proyecto (incluye el YAML `infra/urponline-oneclick.yaml`) |
| `ACCOUNT=` / `BUCKET=` | Arma un nombre de bucket único con tu número de cuenta |
| `DBPASS=` | Genera la contraseña de RDS y la guarda en `~/urponline-dbpass.txt` |
| `curl ...` | Descarga el ZIP compilado de la tienda |
| `aws s3 mb` / `aws s3 cp` | Crea el bucket y sube el ZIP (las EC2 lo descargan al arrancar) |
| `aws cloudformation deploy` | Entrega el YAML a CloudFormation, que crea VPC, ALB, EC2, RDS, ElastiCache y S3 (20-30 min) |
| `describe-stacks` | Muestra los resultados: **`ApplicationHTTPS` es la URL segura de tu tienda** (CloudFront, `https://xxxx.cloudfront.net`); `ApplicationHTTP` es la del ALB directo |

Puedes cerrar CloudShell mientras se crea: sigue en AWS. Avance en **CloudFormation → URPOnlineLab → Events**; al terminar, la URL está en la pestaña **Outputs**.

---

Además, hay dos formas de hacerlo desde tu PC. **Haz primero la A** (para entender cada paso); después automatiza con la B.

> 🆓 **Cuentas del plan gratuito de AWS:** la plantilla ya viene ajustada (respaldo de RDS de **1 día**, instancias pequeñas t3.small / db.t3.micro / cache.t3.micro, RDS sin Multi-AZ). Si AWS rechaza algo por el plan gratuito, el motivo aparece en **CloudFormation → Events** (ej.: *exceeds the maximum available to free tier customers*).
>
> ⚠️ **Cuesta dinero mientras exista** (NAT, ALB, EC2, RDS, Redis). Al terminar la clase **bórrala** (ver "Borrar todo" al final de esta sección).

### A) A mano con la AWS CLI

Requisito: haber hecho **"Conectar tu equipo a tu cuenta de AWS"** (arriba). Ejecuta en la terminal de VS Code, desde la carpeta del proyecto:

```powershell
# 0. Variables (cambia la contraseña: 12+ caracteres, solo letras y números)
$REGION = "us-east-2"
$STACK  = "URPOnlineLab"
$DBPASS = "CambiaEstaClave2026"
$ACCOUNT = aws sts get-caller-identity --query Account --output text
$BUCKET = "urponline-artefactos-$ACCOUNT-$REGION"

# 1. Compilar para Linux (las EC2 usan Amazon Linux) e incluir el modelo de búsqueda
powershell -ExecutionPolicy Bypass -File scripts/descargar-modelo.ps1
dotnet publish TiendaOnline.csproj -c Release -r linux-x64 --self-contained false -o out
Compress-Archive -Path out\* -DestinationPath urponline-publish.zip -Force

# 2. Subir el ZIP a S3 (las EC2 lo descargan al arrancar)
aws s3 mb "s3://$BUCKET" --region $REGION
aws s3 cp urponline-publish.zip "s3://$BUCKET/releases/urponline-v1.zip"

# 3. Validar el YAML y crear la pila (15-30 min)
aws cloudformation validate-template --template-body file://infra/urponline-oneclick.yaml
aws cloudformation deploy --stack-name $STACK --region $REGION `
  --template-file infra/urponline-oneclick.yaml --capabilities CAPABILITY_IAM `
  --parameter-overrides ArtifactBucket=$BUCKET ArtifactKey=releases/urponline-v1.zip DBPassword=$DBPASS EnableMultiAZ=false

# 4. Ver el resultado
aws cloudformation describe-stacks --stack-name $STACK --region $REGION --query "Stacks[0].Outputs" --output table
```

Mientras se crea, míralo en la consola: **CloudFormation → URPOnlineLab → pestaña Events** (cada recurso aparece al crearse).

**Comprobar que funciona en AWS:**

```powershell
$URL = aws cloudformation describe-stacks --stack-name $STACK --region $REGION --query "Stacks[0].Outputs[?OutputKey=='ApplicationHTTP'].OutputValue" --output text
$TG  = aws cloudformation describe-stacks --stack-name $STACK --region $REGION --query "Stacks[0].Outputs[?OutputKey=='TargetGroupArn'].OutputValue" --output text

curl.exe "$URL/health"                                         # {"status":"ok"}
aws elbv2 describe-target-health --target-group-arn $TG --region $REGION --output table   # 2 EC2 "healthy"
1..6 | % { (curl.exe -s $URL) -match 'Servido por: <b>([^<]*)' | Out-Null; $Matches[1] }   # el ALB reparte entre las 2 EC2
start $URL                                                      # abre la tienda; busca "tomar fotos" (pgvector en RDS)
```

El pie de cada página dice **"Servido por: ip-10-0-11-x"** o **"ip-10-0-12-x"**: es la EC2 que te atendió. Recarga y verás el balanceo del ALB, y el carrito se mantiene porque la sesión vive en **ElastiCache**.

**Publicar una versión nueva:** repite el paso 1 y 2 con otro nombre (`urponline-v2.zip`) y el paso 3 con `ArtifactKey=releases/urponline-v2.zip`. CloudFormation reemplaza las EC2 **de una en una** sin tumbar la tienda.

### B) Automático con GitHub Actions: CI/CD (cada cambio se compila, se valida y se despliega solo)

#### ¿Qué es CI/CD y para qué sirve?

- **CI — Integración Continua:** cada vez que alguien sube un cambio a GitHub (`git push`), un servidor de GitHub **compila la aplicación y revisa la plantilla YAML automáticamente**. Si algo está roto, lo marca con una ❌ roja en minutos, **antes** de que llegue a AWS.
- **CD — Despliegue Continuo:** si la compilación sale bien, **el mismo servidor sube la nueva versión a AWS** (S3 + CloudFormation) sin que nadie ejecute comandos a mano.

**¿Por qué importa?** Sin CI/CD, cada versión exige repetir a mano los pasos de la opción A (compilar, zip, subir a S3, `cloudformation deploy`): es lento y es fácil equivocarse. Con CI/CD: **cambias código → `git push` → en ~20 min la tienda nueva está en AWS**, y cada paso queda registrado (quién, cuándo, qué se desplegó).

```
 Tú (VS Code)          GitHub                          AWS
 ───────────           ──────────────────────          ──────────────────────────
 git push   ───────►   GitHub Actions
                        1. compila .NET  (CI)
                        2. valida el YAML (CI)
                        3. arma el ZIP
                        4. sube el ZIP  ─────────────►  S3
                        5. cloudformation deploy ────►  CloudFormation actualiza
                                                        EC2 (de una en una), ALB...
                        6. prueba /health ◄───────────  ALB responde
                        ✅ / ❌ en la pestaña Actions
```

#### Primer despliegue vs. los siguientes

| | Qué hace CloudFormation | Tiempo |
|---|---|---|
| **Primer despliegue** | Crea **todo**: VPC, subredes, NAT, ALB, RDS, ElastiCache, S3, rol IAM y las EC2 | 20-30 min |
| **Cada cambio siguiente** (`git push`) | Compara el YAML y ve que solo cambió el ZIP de la app → **solo reemplaza las EC2, de una en una** (rolling update). VPC, RDS (tus datos), Redis y ALB **no se tocan** | 10-15 min, sin caída |

Si cambias el **YAML** (por ejemplo, agregas un recurso), CloudFormation actualiza solo ese recurso.

#### ¿Qué es GitHub Actions?

Es el servicio de GitHub que ejecuta esos pasos. Las instrucciones viven en archivos **YAML** dentro de [`.github/workflows/`](.github/workflows/) (cada archivo es un *workflow*). Este proyecto trae 3:

| Archivo | Nombre en GitHub | Cuándo corre | Qué hace | ¿Usa AWS? |
|---|---|---|---|---|
| `ci.yml` | *CI - compilar y validar* | Cada `git push` y cada pull request | Compila .NET y valida los YAML de CloudFormation con `cfn-lint` | No (gratis) |
| `deploy-aws.yml` | *Desplegar en AWS* | Cada `git push` a `main` (o con el botón **Run workflow**) | Compila para Linux, sube el ZIP a S3, ejecuta `aws cloudformation deploy`, prueba `/health` en el ALB y muestra la URL | Sí |
| `destroy-aws.yml` | *Destruir AWS* | Solo a mano, escribiendo `DESTRUIR` | Borra la pila para dejar de pagar | Sí |

> Los workflows ejecutan **exactamente los mismos comandos de la AWS CLI** de la opción A; solo que los corre un servidor de GitHub en vez de tu PC. Ábrelos en VS Code: cada paso tiene un `name:` que explica qué hace.

#### ¿Cómo entra GitHub a tu cuenta de AWS? (OIDC, sin Access Keys)

GitHub necesita permiso para crear recursos en **tu** cuenta. En lugar de guardar tu Access Key en GitHub (peligroso si se filtra), se usa **OIDC**: creas en AWS un **rol IAM** que dice *"confío en GitHub, pero solo en la rama `main` del repositorio `TU_USUARIO/tiendaonline`"*. En cada ejecución GitHub pide a AWS credenciales **temporales** (duran minutos) para ese rol. Ese rol lo crea la plantilla [`infra/github-oidc.yaml`](infra/github-oidc.yaml).

#### Configúralo en TU GitHub, paso a paso (una sola vez, ~15 min)

**Paso 1 — Crea tu copia del repositorio (fork).**
Entra a https://github.com/jjjfrancia/tiendaonline (con tu cuenta de GitHub iniciada) → botón **Fork** (arriba a la derecha) → **Create fork**. Ahora tienes `https://github.com/TU_USUARIO/tiendaonline`: ahí correrán **tus** workflows contra **tu** AWS.

**Paso 2 — Crea en AWS el rol para GitHub (OIDC).** Elige una forma:

- **Con la consola de AWS (sin terminal):**
  1. Descarga el archivo [`infra/github-oidc.yaml`](infra/github-oidc.yaml) (botón *Download raw file*).
  2. Consola de AWS (región **Ohio**) → **CloudFormation → Create stack → With new resources** → **Upload a template file** → elige `github-oidc.yaml` → **Next**.
  3. **Stack name:** `urponline-github-oidc` · **GitHubOwner:** tu usuario de GitHub · **RepoName:** `tiendaonline` → **Next** → **Next**.
  4. Marca *"I acknowledge that AWS CloudFormation might create IAM resources with custom names"* → **Submit**.
  5. Cuando diga `CREATE_COMPLETE` (≈1 min) → pestaña **Outputs** → copia el valor de **`RoleArn`** (`arn:aws:iam::123456789012:role/urponline-github-deploy-tiendaonline`).
- **Con CloudShell** (icono `>_` en la consola de AWS):
  ```bash
  git clone https://github.com/TU_USUARIO/tiendaonline.git && cd tiendaonline
  aws cloudformation deploy --stack-name urponline-github-oidc --region us-east-2 \
    --template-file infra/github-oidc.yaml --capabilities CAPABILITY_NAMED_IAM \
    --parameter-overrides GitHubOwner=TU_USUARIO RepoName=tiendaonline
  aws cloudformation describe-stacks --stack-name urponline-github-oidc --region us-east-2 \
    --query "Stacks[0].Outputs[0].OutputValue" --output text
  ```

> Si sale *"Provider with url https://token.actions.githubusercontent.com already exists"*: tu cuenta ya tenía la conexión. Repite con el parámetro **CreateOidcProvider = false**.

**Paso 3 — Guarda 2 secretos en tu fork.** Los *secrets* son valores privados que los workflows pueden usar pero que nadie ve (ni en los logs).
En GitHub → **tu** repositorio → **Settings** (pestaña de arriba) → menú izquierdo **Secrets and variables → Actions** → botón **New repository secret**. Crea estos dos:

| Name | Secret |
|---|---|
| `AWS_ROLE_ARN` | el `RoleArn` del paso 2 |
| `DB_PASSWORD` | una contraseña para la base de datos RDS: **12 o más caracteres, solo letras y números** (ej. `UrpTienda2026Clave`) |

(Opcional) Si no usas Ohio: pestaña **Variables** → **New repository variable** → `AWS_REGION` = `us-east-1`.

**Paso 4 — Activa Actions en tu fork.** En los forks GitHub los deja apagados por seguridad: pestaña **Actions** → botón verde *"I understand my workflows, go ahead and enable them"*.

**Paso 5 — Primer despliegue.** Pestaña **Actions** → en la izquierda **Desplegar en AWS** → botón **Run workflow** → **Run workflow**. Haz clic en la ejecución que aparece para ver cada paso en vivo (≈25 min la primera vez, porque crea toda la infraestructura). Al terminar, abre el **Summary**: muestra la **URL de la tienda** y las EC2 *healthy* detrás del ALB.

**Paso 6 — Comprueba el CI/CD con un cambio real.** En VS Code (con tu fork clonado) cambia un texto, por ejemplo en `Views/Home/Index.cshtml`, y súbelo:

```powershell
git add .
git commit -m "Cambio mi primer texto"
git push
```

En **Actions** verás arrancar solos *CI - compilar y validar* y *Desplegar en AWS*. Cuando terminen, recarga la tienda: tu cambio está en AWS. Las EC2 se reemplazaron **de una en una** (Auto Scaling rolling update), así que la tienda nunca dejó de responder.

#### Alternativa: configurarlo todo desde VS Code (terminal + GitHub CLI)

Si prefieres no usar la web de GitHub, los pasos 1, 3, 4 y 5 se hacen con la **GitHub CLI (`gh`)** en la terminal de VS Code:

```powershell
# Instalar GitHub CLI (una vez) y cerrar/abrir VS Code
winget install GitHub.cli

# Iniciar sesión en GitHub con permiso para workflows (abre el navegador: pega el código que muestra y "Authorize")
gh auth login --web --scopes workflow

# Paso 1: crear tu fork y clonarlo en la carpeta actual
gh repo fork jjjfrancia/tiendaonline --clone
cd tiendaonline
code . -r

# Paso 2 (AWS): crear el rol OIDC — mismos comandos de la sección anterior, con GitHubOwner=TU_USUARIO

# Paso 3: guardar los 2 secretos (te pide pegar cada valor; no se muestran)
gh secret set AWS_ROLE_ARN
gh secret set DB_PASSWORD

# Paso 4: activar los workflows del fork
gh workflow enable ci.yml
gh workflow enable deploy-aws.yml
gh workflow enable destroy-aws.yml

# Paso 5: lanzar el despliegue y verlo en vivo en la terminal
gh workflow run deploy-aws.yml
gh run watch

# Al terminar la clase: borrar todo
gh workflow run destroy-aws.yml -f confirmar=DESTRUIR
```

**Ver las ejecuciones dentro de VS Code:** instala la extensión **GitHub Actions** (`github.vscode-github-actions`; VS Code la sugiere al abrir el proyecto). En la barra lateral aparece un icono de **GitHub Actions**: muestra cada workflow, su estado ✅/❌ y los logs de cada paso, y permite relanzarlos con clic derecho → **Run workflow**.

#### Leer los resultados

| Ves | Significa | Qué hacer |
|---|---|---|
| ✅ verde | Todo salió bien | Abre el **Summary** para la URL |
| ❌ rojo en *CI* | El código no compila o el YAML tiene un error | Clic en el paso rojo → lee el mensaje → corrige → `git push` |
| ❌ *Falta el secreto AWS_ROLE_ARN* | No hiciste el paso 3 | Crea el secreto y pulsa **Re-run jobs** |
| ❌ *Not authorized to perform sts:AssumeRoleWithWebIdentity* | `GitHubOwner` del paso 2 no coincide con tu usuario, o el push no fue a `main` | Revisa el usuario en la pila `urponline-github-oidc` |
| ❌ en *Crear o actualizar la pila* | CloudFormation falló | Consola de AWS → CloudFormation → `URPOnlineLab` → **Events** → la primera línea roja dice el motivo |

#### Al terminar la clase

Pestaña **Actions** → **Destruir AWS** → **Run workflow** → escribe `DESTRUIR` → **Run workflow**. Borra la pila para no seguir gastando.

**¿Por qué OIDC y no Access Keys?** GitHub pide a AWS credenciales **temporales** en cada ejecución; el rol solo acepta la rama `main` de **tu** repositorio. No hay claves guardadas que se puedan filtrar.

### Borrar todo (¡obligatorio al terminar!)

- **Con GitHub Actions:** pestaña **Actions → Destruir AWS → Run workflow** → escribe `DESTRUIR`.
- **Con la CLI:**
  ```powershell
  aws cloudformation delete-stack --stack-name URPOnlineLab --region us-east-2
  aws cloudformation wait stack-delete-complete --stack-name URPOnlineLab --region us-east-2
  ```
  Si falla por el bucket de proveedores, vacíalo en la consola (S3 → bucket → **Empty**) y repite.
- Después: borra el **snapshot final de RDS** (RDS → Snapshots) y el bucket de artefactos (`aws s3 rb s3://$BUCKET --force`). Revisa **Billing** al día siguiente.

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

### Librerías .NET (NuGet)

**No se instalan a mano**: están declaradas en `TiendaOnline.csproj` y `dotnet run` / `dotnet restore` las descarga solas.

| Paquete | Para qué |
|---|---|
| `Npgsql.EntityFrameworkCore.PostgreSQL` | EF Core + PostgreSQL (RDS) |
| `Microsoft.EntityFrameworkCore.Design` | Crear y aplicar migraciones |
| `Microsoft.Extensions.Caching.StackExchangeRedis` | Sesión y carrito en Redis (ElastiCache) |
| `AWSSDK.Extensions.NETCore.Setup`, `AWSSDK.S3`, `AWSSDK.SecretsManager`, `AWSSDK.CloudWatch` | **AWS SDK for .NET** |
| `Microsoft.ML.OnnxRuntime`, `Microsoft.ML.Tokenizers` | Modelo de embeddings local (búsqueda semántica) |

### Base de datos: migraciones EF Core (tablas + pgvector + índice HNSW)

Las migraciones están en `Data/Migrations/`:

| Migración | Crea |
|---|---|
| `Inicial` | Tablas `Products`, `Orders`, `OrderItems`, `Promotions`, `BusinessEvents`, `SupplierImports` |
| `BusquedaVectorialPgvector` | Extensión `vector`, tabla `ProductEmbeddings` (`vector(384)`) e **índice HNSW** `vector_cosine_ops` |

Se aplican de **una** de estas dos formas:

```powershell
# A) Automática: al arrancar con InitializeDatabase=true (aplica migraciones + carga productos de ejemplo)
$env:InitializeDatabase = "true"; dotnet run --urls http://localhost:8080

# B) Manual, con la herramienta dotnet-ef (incluida en dotnet-tools.json del repo)
dotnet tool restore
dotnet tool run dotnet-ef database update
```

Comprueba en PostgreSQL:

```powershell
docker exec urp-postgres psql -U urponline -d urponline -c 'SELECT "MigrationId" FROM "__EFMigrationsHistory";'
docker exec urp-postgres psql -U urponline -d urponline -c "SELECT indexname FROM pg_indexes WHERE tablename='ProductEmbeddings';"
```

Debe listar `20261010184951_Inicial`, `20261010185028_BusquedaVectorialPgvector` y el índice `IX_ProductEmbeddings_Embedding_hnsw`.

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
