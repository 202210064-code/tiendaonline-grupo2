# URPOnline — Laboratorio AWS (TiendaOnline .NET)

# 📘 BALOTARIO DEL EXAMEN PARCIAL: 50 PREGUNTAS RESUELTAS

## ➡️ [Descargar el balotario en PDF](RUTA_DEL_CURSO/01_CASO_Y_ARQUITECTURA/Balotario_50_preguntas_SOLUCIONADO.pdf) · [en Word](RUTA_DEL_CURSO/01_CASO_Y_ARQUITECTURA/URPOnline_Navidad_CyberMonday_Catalogo_Nocturno_40_Preguntas.docx)

Las 50 preguntas con la explicación de cada respuesta y, al final, las **definiciones** de todos los servicios, siglas y términos que aparecen en el examen. Quien estudia el balotario puede responder el examen.


## ➡️ [Descargar el solucionario del examen parcial en PDF](RUTA_DEL_CURSO/01_CASO_Y_ARQUITECTURA/Solucionario_Examen_Parcial_AWS_2026-II.pdf)

Las 20 preguntas del examen con su respuesta, la explicación y figuras de apoyo (las pantallas de AWS son recreaciones didácticas).

---

## 📅 Calendario de fases

| Fase | Fecha | Qué se hace | Rama |
|---|---|---|---|
| **Fase 1** | 24 de octubre | Base de AWS: despliegue por pipeline, AWS SDK sin claves, tolerancia a fallas (servidor y zona), PostgreSQL con pgvector, carrito y pago. | [`main`](https://github.com/cortexgovernor/tiendaonline/tree/main) y [`fase-1`](https://github.com/cortexgovernor/tiendaonline/tree/fase-1) |
| **Fase 2** | 7 de noviembre | Agentes en la tienda: los 4 agentes de IA y RAG, arquitectura serverless (EventBridge, SQS, Lambda) y seguridad con intrusión simulada. | [`fase-2`](https://github.com/cortexgovernor/tiendaonline/tree/fase-2) |
| **Fase 3** | 21 de noviembre | IA, apps móviles y contenedores: más patrones de agentes, JSON en el prompt, app móvil que consume la API, y Docker/Fargate/EKS con blue-green. | [`fase-3`](https://github.com/cortexgovernor/tiendaonline/tree/fase-3) |
| **Fase 4** | 5 de diciembre | Casos completos y sistemas distribuidos: cada grupo desarrolla un caso (ONPE, etc.) y MapReduce/Raft con validación automática. | [`fase-4`](https://github.com/cortexgovernor/tiendaonline/tree/fase-4) |

## 🎤 Orden de las presentaciones

| Situación | Regla |
|---|---|
| Orden del turno | Primero los **voluntarios**; si no hay voluntarios, por **sorteo**. |
| No están al ser llamados | **Pierden la presentación.** |
| Grupo incompleto en la sustentación | **Solo tienen nota** los que participen en la presentación en la clase. |

---

# 🧑‍🤝‍🧑 PRIMERO: CREA EL REPOSITORIO DE TU GRUPO. TRABAJA SOLO EN ÉL

Este repositorio del curso es **solo de lectura**: aquí no puedes subir cambios ni desplegar. Cada grupo crea **su propio repositorio**, sube ahí una copia del laboratorio y trabaja solo en él: ahí corren **sus** GitHub Actions contra **su** cuenta de AWS, y ese repositorio es el que se evalúa.

1. Acepta la invitación que te llegó al correo de la universidad (o entra a https://github.com/orgs/cortexgovernor/invitation): sin ella no puedes clonar el laboratorio.
2. **Un integrante crea el repositorio del grupo:** en GitHub, **+ → New repository** → *Owner*: su cuenta → *Repository name*: `tiendaonline-grupoN` (N = número del grupo) → **Private** → **sin** README, `.gitignore` ni licencia → **Create repository**.
3. **Ese integrante clona el laboratorio del curso y lo sube al repositorio del grupo** (terminal de VS Code):
   ```bash
   git clone --branch main https://github.com/cortexgovernor/tiendaonline.git tiendaonline-grupoN
   cd tiendaonline-grupoN
   git branch -M main                       # la fase queda como rama main del grupo
   git remote rename origin curso           # el repositorio del curso queda como "curso" (solo para leer)
   git remote add origin https://github.com/TU_USUARIO/tiendaonline-grupoN.git
   git push -u origin main
   ```
4. **Agrega a los demás integrantes y al profesor** como colaboradores (**Settings → Collaborators → Add people**). Cada integrante lo clona: `git clone https://github.com/TU_USUARIO/tiendaonline-grupoN.git`.
5. El enlace de **ese** repositorio es el que se entrega al profesor.

> Si el curso publica una corrección de la fase: `git pull curso main` y luego `git push`.

---

# 📋 RÚBRICA DE EVALUACIÓN (20 PUNTOS)

## ➡️ [Descargar la rúbrica: `RUTA_DEL_CURSO/07_EVALUACION/URPOnline_Rubrica_Evaluacion_20_Puntos.docx`](RUTA_DEL_CURSO/07_EVALUACION/URPOnline_Rubrica_Evaluacion_20_Puntos.docx)

### 👥 Grupos de **7 a 10** integrantes · 🎤 **100 minutos** de presentación repartidos por igual entre los grupos · ⏰ Entrega del video: viernes 23 de octubre de 2026 · participan **todos** los integrantes

| Bloque | Qué se evalúa (indicadores y criterio de cumplimiento) | Puntos |
|---|---|---|
| **1. Despliegue, AWS SDK y fallas** | **1a Pipeline:** tienda por HTTPS con balanceo, búsqueda y compra desplegada por el pipeline, y un cambio que llega solo. **1b AWS SDK sin claves:** métricas desde AWS, EC2 sin claves y rol IAM explicado. **1c Cae un servidor:** la tienda sigue y el carrito se mantiene (Redis). **1d Cae una zona:** zona A aislada, venta desde la zona B y failover de Redis. **1e Evidencia y restauración** (Logs Insights, alarma, Reachability Analyzer). | 5 o 0 |
| **2. PostgreSQL: carga y búsqueda por vectores** | **2a PgBouncer** desplegado por el pipeline. **2b Prueba de carga k6** (sin y con PgBouncer). **2c Evidencia en CloudWatch** e interpretación. **2d +30 productos** por migración, todos con vector, y consulta de similitud. **2e Experimento de búsqueda** con dos umbrales (0,3 y 0,6) e interpretación. | 5 o 0 |
| **3. Carrito y pago** | **3a Métricas del carrito** (ActiveCarts cuenta personas, no clics). **3b Gráfica, alarma y simulación** desde el YAML. **3c Credenciales de pago** solo en Secrets Manager. **3d Pago con tarjeta de prueba** (ciclo completo). **3e Confirmación segura** (consulta el pago desde el servidor, no duplica). | 5 o 0 |
| **4. Sustentación individual** | Banco de **50 preguntas** y **un caso sorteado de «Del examen a la práctica»** (evidencia de su cuenta y explicación); se sortean por integrante. Si un integrante no sustenta, el bloque vale 0 y ese alumno tiene 0. | 5 o 0 |

**Cada bloque es todo o nada.** **El alumno que no sustenta tiene nota 0**, aunque haya participado en el trabajo. **Requisito para presentar:** los **10 casos de «Del examen a la práctica»** simulados y explicados en el video. La rúbrica explica qué hacer, cómo demostrarlo y dónde cambiar el código, e incluye guías y scripts.

## 💬 [Canal del curso: GitHub Discussions](https://github.com/cortexgovernor/tiendaonline/discussions)

Aquí pueden compartir todo: discutir las respuestas del banco de preguntas, dudas sobre la rúbrica y los desafíos, código, información, soluciones a errores y sugerencias. **Nunca publiquen secretos** (contraseñas, claves de AWS, tokens).

---

## 👉 Empieza aquí

1. **Prepara tu equipo:** [CONFIGURACION_VSCODE.md](CONFIGURACION_VSCODE.md) — qué instalar y configurar (VS Code, .NET 8, AWS CLI, extensiones).
2. **Sigue la ruta del curso en orden:** carpeta [`RUTA_DEL_CURSO/`](RUTA_DEL_CURSO/) — abre primero `URPOnline_INICIO_AQUI.docx`.

| Paso | Carpeta | Contenido |
|---|---|---|
| 01 | `RUTA_DEL_CURSO/01_CASO_Y_ARQUITECTURA` | Caso empresarial, arquitectura y dashboard de analítica |
| 02 | `RUTA_DEL_CURSO/02_APLICACION` | La aplicación C# (código en la raíz de este repo) |
| 03 | `RUTA_DEL_CURSO/03_INSTALACION_Y_AWS_SDK` | Configuración local y AWS SDK (simulacro de 40 preguntas) |
| 04 | `RUTA_DEL_CURSO/04_SESIONES_REDIS` | Sesiones con Redis |
| 05 | `RUTA_DEL_CURSO/05_CLAUDE_CODE_Y_DESPLIEGUE` | Claude Code + CloudFormation → [MEMORIA_DESPLIEGUE.md](MEMORIA_DESPLIEGUE.md) |
| 06 | `RUTA_DEL_CURSO/06_DESAFIOS_AVANZADOS` | Ingeniería distribuida y pruebas de caos |
| 07 | `RUTA_DEL_CURSO/07_EVALUACION` | **Rúbrica de evaluación (20 puntos)** |

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

**CloudShell** es una terminal dentro de la consola de AWS que ya trae la AWS CLI conectada a tu cuenta.

> El repositorio del grupo es **privado**: CloudShell no puede clonarlo solo (no tiene tu sesión de GitHub). Por eso los dos archivos se descargan primero en tu navegador, donde sí iniciaste sesión, y se suben a CloudShell. No uses tokens ni contraseñas de GitHub en CloudShell.

1. **En tu navegador, con tu cuenta de GitHub iniciada**, descarga estos dos archivos a tu PC:
   - El ZIP compilado de la tienda: [Releases v1.0](https://github.com/cortexgovernor/tiendaonline/releases/tag/v1.0) → **`urponline-publish.zip`**.
   - La plantilla, desde el repositorio de **tu grupo**: `infra/urponline-oneclick.yaml` → botón **Download raw file**.
2. Consola de AWS → arriba a la derecha elige la región **Ohio (us-east-2)**.
3. Haz clic en el icono **`>_`** de la barra superior (o busca **CloudShell**).
4. En CloudShell: **Acciones → Cargar archivo** → sube `urponline-publish.zip`. Repite con `urponline-oneclick.yaml`. Quedan en tu carpeta de inicio (`~`).
5. Pega este bloque y presiona **Enter**:

```bash
cd ~ && ls urponline-publish.zip urponline-oneclick.yaml
REGION=us-east-2; STACK=URPOnlineLab
ACCOUNT=$(aws sts get-caller-identity --query Account --output text)
BUCKET=urponline-artefactos-$ACCOUNT-$REGION
DBPASS=$(tr -dc A-Za-z0-9 </dev/urandom | head -c 20); echo "$DBPASS" > ~/urponline-dbpass.txt
aws s3 mb s3://$BUCKET --region $REGION; aws s3 cp urponline-publish.zip s3://$BUCKET/releases/urponline-v1.zip
aws cloudformation deploy --stack-name $STACK --region $REGION --template-file urponline-oneclick.yaml --capabilities CAPABILITY_IAM --parameter-overrides ArtifactBucket=$BUCKET ArtifactKey=releases/urponline-v1.zip DBPassword=$DBPASS EnableMultiAZ=false
aws cloudformation describe-stacks --stack-name $STACK --region $REGION --query "Stacks[0].Outputs" --output table
```

| Línea | Qué hace |
|---|---|
| `ls ...` | Comprueba que los dos archivos se subieron (si dice *No such file*, repite el paso 4) |
| `ACCOUNT=` / `BUCKET=` | Arma un nombre de bucket único con tu número de cuenta |
| `DBPASS=` | Genera la contraseña de RDS y la guarda en `~/urponline-dbpass.txt` |
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
tar.exe -a -c -f urponline-publish.zip -C out .   # rutas con «/» (Compress-Archive de PowerShell 5.1 usa «\» y Linux lo marca como error)

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

GitHub necesita permiso para crear recursos en **tu** cuenta. En lugar de guardar tu Access Key en GitHub (peligroso si se filtra), se usa **OIDC**: creas en AWS un **rol IAM** que dice *"confío en GitHub, pero solo en la rama `main` del repositorio `TU_USUARIO/tiendaonline-grupoN`"*. En cada ejecución GitHub pide a AWS credenciales **temporales** (duran minutos) para ese rol. Ese rol lo crea la plantilla [`infra/github-oidc.yaml`](infra/github-oidc.yaml).

#### Configúralo en TU GitHub, paso a paso (una sola vez, ~15 min)

**Paso 1 — El repositorio del grupo.**
Es el que crearon al inicio ("Crea el repositorio de tu grupo"): `https://github.com/TU_USUARIO/tiendaonline-grupoN`. Ahí correrán **sus** workflows contra **su** AWS.

**Paso 2 — Crea en AWS el rol para GitHub (OIDC).** Elige una forma:

- **Con la consola de AWS (sin terminal):**
  1. Descarga el archivo [`infra/github-oidc.yaml`](infra/github-oidc.yaml) (botón *Download raw file*).
  2. Consola de AWS (región **Ohio**) → **CloudFormation → Create stack → With new resources** → **Upload a template file** → elige `github-oidc.yaml` → **Next**.
  3. **Stack name:** `urponline-github-oidc` · **GitHubOwner:** tu usuario de GitHub · **RepoName:** `tiendaonline-grupoN` (el nombre de **su** repositorio) → **Next** → **Next**.
  4. Marca *"I acknowledge that AWS CloudFormation might create IAM resources with custom names"* → **Submit**.
  5. Cuando diga `CREATE_COMPLETE` (≈1 min) → pestaña **Outputs** → copia el valor de **`RoleArn`** (`arn:aws:iam::123456789012:role/urponline-github-deploy-tiendaonline`).
- **Con CloudShell** (icono `>_` en la consola de AWS):
  ```bash
  git clone https://github.com/TU_USUARIO/tiendaonline-grupoN.git && cd tiendaonline-grupoN
  aws cloudformation deploy --stack-name urponline-github-oidc --region us-east-2 \
    --template-file infra/github-oidc.yaml --capabilities CAPABILITY_NAMED_IAM \
    --parameter-overrides GitHubOwner=TU_USUARIO RepoName=tiendaonline-grupoN
  aws cloudformation describe-stacks --stack-name urponline-github-oidc --region us-east-2 \
    --query "Stacks[0].Outputs[0].OutputValue" --output text
  ```

> Si sale *"Provider with url https://token.actions.githubusercontent.com already exists"*: tu cuenta ya tenía la conexión. Repite con el parámetro **CreateOidcProvider = false**.

**Paso 3 — Guarda 2 secretos en el repositorio del grupo.** Los *secrets* son valores privados que los workflows pueden usar pero que nadie ve (ni en los logs).
En GitHub → **tu** repositorio → **Settings** (pestaña de arriba) → menú izquierdo **Secrets and variables → Actions** → botón **New repository secret**. Crea estos dos:

| Name | Secret |
|---|---|
| `AWS_ROLE_ARN` | el `RoleArn` del paso 2 |
| `DB_PASSWORD` | una contraseña para la base de datos RDS: **12 o más caracteres, solo letras y números** (ej. `UrpTienda2026Clave`) |

(Opcional) Si no usas Ohio: pestaña **Variables** → **New repository variable** → `AWS_REGION` = `us-east-1`.

**Paso 4 — Comprueba que Actions esté activo.** En un repositorio nuevo viene activado: pestaña **Actions** → deben aparecer los 3 workflows. Si dice que están desactivados, pulsa **Enable**.

**Paso 5 — Primer despliegue.** Pestaña **Actions** → en la izquierda **Desplegar en AWS** → botón **Run workflow** → **Run workflow**. Haz clic en la ejecución que aparece para ver cada paso en vivo (≈25 min la primera vez, porque crea toda la infraestructura). Al terminar, abre el **Summary**: muestra la **URL de la tienda** y las EC2 *healthy* detrás del ALB.

**Paso 6 — Comprueba el CI/CD con un cambio real.** En VS Code (con el repositorio del grupo clonado) cambia un texto, por ejemplo en `Views/Home/Index.cshtml`, y súbelo:

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

# Paso 1: clonar el laboratorio y crear con él el repositorio privado del grupo
git clone --branch main https://github.com/cortexgovernor/tiendaonline.git tiendaonline-grupoN
cd tiendaonline-grupoN
git branch -M main && git remote rename origin curso
gh repo create TU_USUARIO/tiendaonline-grupoN --private --source . --remote origin --push
code . -r

# Paso 2 (AWS): crear el rol OIDC — mismos comandos de la sección anterior, con GitHubOwner=TU_USUARIO

# Paso 3: guardar los 2 secretos (te pide pegar cada valor; no se muestran)
gh secret set AWS_ROLE_ARN
gh secret set DB_PASSWORD

# Paso 4: activar los workflows (en un repositorio nuevo ya lo están; no hace daño)
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

### 📊 Telemetría: ver todos los servicios funcionando en UNA pantalla

La plantilla crea un **dashboard de Amazon CloudWatch** llamado `URPOnline-URPOnlineLab` con todos los servicios juntos. Ábrelo desde **CloudFormation → URPOnlineLab → Outputs → `DashboardURL`** (o CloudWatch → **Dashboards**).

| Panel | Servicio | Qué demuestra |
|---|---|---|
| Alarmas | CloudWatch Alarms | Verde = todo bien. Rojo si una EC2 no responde o hay errores 5xx |
| Peticiones y errores 5xx · Tiempo de respuesta | **ALB** | La tienda recibe tráfico y responde rápido |
| EC2 sanas detrás del balanceador | **ALB + Auto Scaling** | Hay 2 EC2 *healthy* |
| CPU % y memoria % | **EC2** (agente de CloudWatch) | Carga de los servidores |
| Conexiones y CPU % | **RDS PostgreSQL** | La base de datos en uso |
| Conexiones y memoria % | **ElastiCache Redis** | Sesiones y carritos guardados |
| Pedidos y ventas S/ | **La app con el AWS SDK** (`PutMetricData`) | La aplicación escribe en CloudWatch desde el código |
| Peticiones y % de errores | **CloudFront** | El HTTPS y la caché |
| Logs de la tienda | **CloudWatch Logs** | Cada línea con la EC2 que la generó |
| Peticiones por EC2 | **CloudWatch Logs (Nginx)** | El ALB reparte entre las 2 EC2 |
| Tráfico rechazado | **VPC Flow Logs** | Los security groups bloquean lo que no debe pasar |

**Demo en clase (10 min):**

1. Abre el dashboard y, en otra pestaña, la tienda (`ApplicationHTTPS`).
2. Navega, busca (`tomar fotos`, `reloj deportivo`), agrega al carrito y **compra**. Repite 3 o 4 veces.
3. En 1-2 minutos (botón ⟳ del dashboard) suben **Peticiones**, **Conexiones RDS**, **Redis** y **Pedidos / Ventas**; en *Logs de la tienda* aparecen tus peticiones con el ID de la EC2.
4. **Prueba de alta disponibilidad:** consola → **EC2 → Instances** → elige una de las 2 → **Instance state → Terminate**. En el dashboard: *EC2 sanas* baja a 1 y la alarma se pone roja; Auto Scaling crea otra EC2 y en ~5 min vuelve a 2 y la alarma a verde. **La tienda nunca dejó de responder** y tu carrito sigue ahí (sesión en ElastiCache).

**Reachability Analyzer (seguridad de la red):** en **CloudShell**, dentro de la carpeta del proyecto:

```bash
bash scripts/analizar-red.sh
```

Resultado esperado:

```
ALB -> EC2 : 80 (Nginx)                       ALCANZABLE ✅
EC2 -> RDS : 5432 (PostgreSQL)                ALCANZABLE ✅
Internet -> EC2 : 80 (saltando el ALB)        BLOQUEADO 🔒
Internet -> RDS : 5432                        BLOQUEADO 🔒
```

Demuestra que **solo el ALB** llega a las EC2 y **solo las EC2** llegan a la base de datos. El detalle visual de cada ruta está en **VPC → Reachability Analyzer** (~0,10 USD por análisis).

### 🧩 AWS SDK for .NET: la tienda usa AWS desde su código

**¿Qué es?** Una librería que permite que **la aplicación** (no una persona) use los servicios de AWS desde el código C#.

| Herramienta | Quién la usa | Ejemplo en este curso |
|---|---|---|
| Consola web | Una persona, con clics | Abrir el dashboard de CloudWatch |
| AWS CLI | Una persona o un script, con comandos | `aws cloudformation deploy ...` |
| **AWS SDK** | **La aplicación, desde su código** | La tienda envía a CloudWatch *"se creó un pedido de S/ 3.799"* |

**¿Qué usa la tienda del SDK?**

| Paquete NuGet | Servicio | Para qué lo usa la tienda | Estado |
|---|---|---|---|
| `AWSSDK.CloudWatch` | **Amazon CloudWatch** | En cada compra publica las métricas **`OrdersCreated`** (pedidos) y **`OrderAmount`** (ventas S/) → panel *"Tienda (AWS SDK): pedidos y ventas S/"* del dashboard | ✅ En uso |
| `AWSSDK.S3` | **Amazon S3** | El **Worker** lee el CSV nocturno de proveedores (`GetObject`) para actualizar el catálogo | ⚠️ El Worker aún no compila (pendiente) |
| `AWSSDK.SecretsManager` | **AWS Secrets Manager** | Leer la contraseña de RDS de forma segura (en vez de un archivo en la EC2) | ⏳ Registrado, aún sin usar |
| `AWSSDK.Extensions.NETCore.Setup` | — | Conecta el SDK con ASP.NET Core: región y credenciales automáticas | ✅ En uso |

#### El código, paso a paso

**1. Instalar la librería** — [`TiendaOnline.csproj`](TiendaOnline.csproj) (la descarga `dotnet restore`):

```xml
<PackageReference Include="AWSSDK.Extensions.NETCore.Setup" Version="3.7.400" />
<PackageReference Include="AWSSDK.CloudWatch" Version="3.7.401" />
```

**2. Registrar el cliente de CloudWatch** — [`Program.cs`](Program.cs):

```csharp
builder.Services.AddDefaultAWSOptions(builder.Configuration.GetAWSOptions()); // región + credenciales
builder.Services.AddAWSService<IAmazonCloudWatch>();                          // cliente de CloudWatch
builder.Services.AddScoped<BusinessMetrics>();                                // nuestro servicio
```

**3. Llamar a AWS** — [`Services/BusinessMetrics.cs`](Services/BusinessMetrics.cs):

```csharp
public class BusinessMetrics(IAmazonCloudWatch cloudWatch, IConfiguration config, ILogger<BusinessMetrics> log) {
 public async Task OrderCreatedAsync(decimal amount, CancellationToken ct = default) {
  if (!config.GetValue("Metrics:CloudWatchEnabled", false)) return;   // en tu PC queda apagado
  try {
   await cloudWatch.PutMetricDataAsync(new PutMetricDataRequest {      // ← la llamada a AWS
    Namespace = "URPOnline",
    MetricData = new List<MetricDatum> {
     new() { MetricName = "OrdersCreated", Value = 1, Unit = StandardUnit.Count },
     new() { MetricName = "OrderAmount", Value = (double)amount, Unit = StandardUnit.None }
    }
   }, ct);
  } catch (Exception ex) when (ex is not OperationCanceledException) {
   log.LogWarning(ex, "No se pudo publicar la métrica del pedido en CloudWatch"); // la venta ya está en RDS
  }
 }
}
```

**4. Usarlo al comprar** — [`Controllers/CheckoutController.cs`](Controllers/CheckoutController.cs), justo después de guardar el pedido en RDS:

```csharp
await metrics.OrderCreatedAsync(order.Total, HttpContext.RequestAborted);
```

#### ¿Y las credenciales? (no hay claves en el código)

| Dónde corre | De dónde saca los permisos el SDK |
|---|---|
| **EC2 en AWS** | Del **rol IAM** que crea el YAML: [`infra/urponline-oneclick.yaml`](infra/urponline-oneclick.yaml) le da `CloudWatchAgentServerPolicy` (incluye `cloudwatch:PutMetricData`) y activa las métricas con `Metrics__CloudWatchEnabled=true` y `AWS__Region` |
| **Tu PC** | De `aws configure` (`%USERPROFILE%\.aws\credentials`). Para probarlo: `$env:Metrics__CloudWatchEnabled="true"; $env:AWS__Region="us-east-2"; dotnet run --urls http://localhost:8080` |

#### Comprobar que funciona

1. Haz 2 o 3 compras en la tienda.
2. Dashboard → panel **"Tienda (AWS SDK): pedidos y ventas S/"** (pulsa ⟳ tras 1-2 min).
3. O con la CLI:

```bash
aws cloudwatch get-metric-statistics --namespace URPOnline --metric-name OrdersCreated \
  --start-time $(date -u -d '-1 hour' +%FT%TZ) --end-time $(date -u +%FT%TZ) \
  --period 3600 --statistics Sum --region us-east-2
```

### Borrar todo (¡obligatorio al terminar!)

> ⏳ **Borrar la pila demora entre 10 y 15 minutos** (RDS y ElastiCache son los que más tardan). Esperen a que termine (estado `DELETE_COMPLETE` o que la pila desaparezca de CloudFormation) antes de dar por terminado el borrado; si la cierran a medias, quedan recursos cobrando.

- **Con GitHub Actions:** pestaña **Actions → Destruir AWS → Run workflow** → escribe `DESTRUIR`.
- **Con la CLI:**
  ```powershell
  aws cloudformation delete-stack --stack-name URPOnlineLab --region us-east-2
  aws cloudformation wait stack-delete-complete --stack-name URPOnlineLab --region us-east-2
  ```
  Si falla por el bucket de proveedores, vacíalo en la consola (S3 → bucket → **Empty**) y repite.
  El bucket de **CloudTrail** se conserva al borrar la pila (tiene registros): antes del `delete-stack`, anota su nombre (Output `TrailBucketName`) y al final bórralo con `aws s3 rb s3://NOMBRE --force`.
- Después: borra el **snapshot final de RDS** (RDS → Snapshots) y el bucket de artefactos (`aws s3 rb s3://$BUCKET --force`). Revisa **Billing** al día siguiente.

---

## 🧰 dnf: cómo cada EC2 instala sus programas (y en qué se diferencia de SSH)

**dnf** es el **gestor de paquetes de Amazon Linux**, el sistema operativo de las EC2. Funciona como una tienda de aplicaciones del servidor: descarga los programas desde los repositorios de Amazon Linux en Internet, instala también lo que cada uno necesita (sus dependencias), comprueba que vengan firmados y los deja listos.

### Dónde lo usa el laboratorio

Nadie lo escribe a mano: lo ejecuta el **User Data** del Launch Template cada vez que Auto Scaling crea una EC2 (`infra/urponline-oneclick.yaml`):

```bash
dnf install -y aws-cfn-bootstrap
dnf install -y nginx unzip amazon-cloudwatch-agent aspnetcore-runtime-8.0
```

> 📌 **dnf está en la plantilla de CloudFormation** (`infra/urponline-oneclick.yaml`), dentro del **Launch Template → UserData**, y se ejecuta **en forma automática** cada vez que Auto Scaling crea una EC2: nadie lo escribe a mano. El primer paquete que instala, `aws-cfn-bootstrap`, es el paquete de herramientas de **CloudFormation**: trae `cfn-signal`.

| Paquete | Para qué lo necesita la tienda |
|---|---|
| `aws-cfn-bootstrap` | Trae **cfn-signal**: la EC2 avisa a CloudFormation si arrancó bien o mal |
| `nginx` | El **proxy inverso** del puerto 80, que reenvía a Kestrel en 127.0.0.1:8080 |
| `unzip` | Descomprime el **ZIP de la versión** que se descarga de S3 (ArtifactKey) |
| `amazon-cloudwatch-agent` | Envía los **logs y métricas** de la EC2 a CloudWatch |
| `aspnetcore-runtime-8.0` | El **runtime de .NET 8** que ejecuta la aplicación |

`-y` responde «sí» automáticamente, porque el User Data corre sin nadie frente a la pantalla.

### Por qué dnf necesita el NAT Gateway

Las EC2 están en la subred de aplicación **sin IP pública**. dnf **inicia** la conexión hacia los repositorios (**salida**, HTTPS 443): el tráfico sigue la ruta `0.0.0.0/0` de la subred hacia el **NAT Gateway de su zona**, que lo saca a Internet por el IGW, y la respuesta vuelve por el mismo camino. Desde Internet nadie puede iniciar una conexión hacia la EC2 a través del NAT.

Si la instalación falla, Nginx o .NET no quedan instalados y la EC2 no puede atender: CloudFormation no recibe la señal de éxito (**cfn-signal**) y **revierte** la actualización.

Para verlo (opcional), entra a una EC2 por **Session Manager**:

```bash
sudo grep -i "dnf\|Installed\|Complete" /var/log/cloud-init-output.log | head -20   # lo que instaló el User Data
dnf list installed nginx aspnetcore-runtime-8.0                                    # paquetes instalados y su versión
```

### dnf frente a SSH

Son cosas distintas: **dnf instala programas** en la EC2; **SSH es una forma de entrar** a la EC2 para administrarla.

| | **dnf** | **SSH** |
|---|---|---|
| Qué es | Gestor de paquetes de Amazon Linux | Protocolo de terminal remota cifrada (*Secure Shell*) |
| Para qué sirve | Instalar y actualizar programas (Nginx, .NET, parches) | Abrir una terminal en otra máquina para administrarla |
| Dirección de la conexión | **Saliente**: la EC2 se conecta a los repositorios | **Entrante**: alguien desde fuera se conecta a la EC2 |
| Puerto | Sale por **443** (HTTPS) hacia Internet | La EC2 debe tener **abierto el 22** |
| Qué necesita | Ruta de salida (**NAT Gateway**); ninguna regla de entrada en el Security Group | IP alcanzable, regla de **entrada 22** en el Security Group y una **llave privada** |
| En URPOnline | Lo ejecuta el **User Data** al arrancar cada EC2 | **No se usa**: se usa **Session Manager** sin puertos de entrada |

> Session Manager también funciona por **salida**: el agente SSM de la EC2 se conecta a AWS, y por eso no hace falta abrir el 22. Sin IP pública ni puerto 22, SSH ni siquiera podría llegar a las EC2 del laboratorio; dnf sí funciona porque no necesita que nadie entre: es la EC2 la que sale.

## 🧪 Del examen a la práctica: 10 casos (Fase 1)

En el examen parcial estos casos se resolvieron en teoría. Ahora cada grupo los **simula en su propia cuenta de AWS** y los **explica con su evidencia**. Cada caso indica la pregunta del examen de la que sale.

**Cómo se entrega:** cada caso aparece en el video (simulación + evidencia + explicación). En la sustentación, cada integrante defiende **un caso sorteado** mostrando la evidencia de su cuenta (con el nombre de su pila y la hora) y explicando qué pasó y por qué.

> Todos los cambios de los casos se **revierten al terminar**; si un cambio se hizo en la consola, se vuelve a ejecutar el pipeline para dejar la pila como dice la plantilla.

| # | Caso (pregunta del examen) | Cómo lo simulan | Evidencia | Qué explican |
|---|---|---|---|---|
| 1 | **Salida a Internet de las EC2 privadas** (P1) | En **VPC → Route tables**, en la tabla de **una** subred de aplicación, quitan la ruta `0.0.0.0/0 → NAT`. Por **Session Manager**, en una EC2 de esa subred, ejecutan `curl -sI --max-time 10 https://github.com`: no responde. Restauran la ruta y repiten: responde. | Tabla de rutas antes y después; la salida del comando en los dos casos. | Ruta local frente a ruta por defecto; por qué el tráfico a RDS siguió funcionando sin la ruta; por qué hay un NAT por zona. |
| 2 | **El carrito que desaparece** (P2) | En su equipo levantan la tienda **dos veces** (puertos 5001 y 5002) **sin Redis** (`ASPNETCORE_ENVIRONMENT=Development` y `ConnectionStrings__Redis` vacío): agregan un producto en una y abren el carrito en la otra. Repiten **con Redis** (`docker start urp-redis`). La parte de conexiones ya la midieron con PgBouncer (indicadores 2a a 2c). | El carrito vacío sin Redis y compartido con Redis; las claves `URPOnline-DataProtection-Keys` en Redis. | Por qué se comparten la sesión **y** las claves de la cookie; por qué las sticky sessions no bastan; cómo PgBouncer evita saturar RDS. |
| 3 | **Instancias marcadas como no sanas** (P3) | En **EC2 → Target groups → Health checks**, cambian el puerto del chequeo a `8080`. Esperan 1-2 minutos: los targets pasan a **Unhealthy** y la tienda deja de responder. Lo devuelven a *traffic port* (80) y `/health`. | Pestaña **Targets** en los dos estados; la alarma `…-EC2-no-saludables`. | Cómo decide el ALB a quién enviar tráfico; por qué Kestrel en `127.0.0.1:8080` no responde al chequeo; por qué no se abre el 8080. |
| 4 | **Error 502** (P4) | Por Session Manager, en **una** EC2: `sudo systemctl stop urponline` y `curl -sI http://localhost/health` → **502**. Revisan `systemctl status urponline`, `journalctl -u urponline -n 50`, `tail -n 50 /var/log/urponline/app.log` y `grep proxy_pass /etc/nginx/nginx.conf`. Luego `sudo systemctl start urponline`. | El 502 con la firma de Nginx; el estado del servicio; la línea `proxy_pass`; el target que el ALB marcó no sano. | Qué es un proxy inverso; por qué responde Nginx y no la aplicación; por qué el ALB dejó de enviarle clientes. |
| 5 | **RDS rechaza las conexiones** (P5) | En **VPC → Network ACLs**, en la NACL de datos, quitan la regla de **salida** `1024-65535 → 10.0.11.0/24`. La EC2 de AZ-a deja de llegar a RDS y Redis (tiempo de espera); la de AZ-b sigue bien. Restauran la regla. | Errores de la aplicación en AZ-a; en el dashboard, el panel de Flow Logs con `REJECT`. | Security Group stateful frente a NACL stateless; puertos efímeros; por qué solo falló una zona. |
| 6 | **Cae una zona de disponibilidad** (P8) | Con tráfico en la tienda, terminan la EC2 de AZ-a. Miden cuánto tarda Auto Scaling en reponerla y si hubo errores. | **Auto Scaling → Activity**; Targets; errores 5xx del ALB durante el reemplazo. | EC2 sin estado; qué haría RDS Multi-AZ si cayera la base (failover) y por qué en el laboratorio está apagado. |
| 7 | **HTTPS sin dominio propio** (P12) | Abren la tienda por `http://…elb.amazonaws.com` y por `https://…cloudfront.net`. En el navegador revisan el certificado de CloudFront. | El aviso «No seguro» frente al candado; el certificado `*.cloudfront.net`. | Por qué ACM no emite un certificado para la dirección del ALB; qué hará falta cuando haya dominio propio. |
| 8 | **Versión nueva sin detener la tienda** (P13) | Cambian un texto visible, hacen push y, **mientras corre la prueba de carga**, siguen el despliegue. Después buscan «algo para escuchar música». | Eventos de la pila (cfn-signal de cada EC2), Activity de Auto Scaling, errores 5xx en cero; el resultado de la búsqueda. | Rolling update, Launch Template y ArtifactKey; cómo el índice HNSW y la distancia coseno (`<=>`) encuentran el producto. |
| 9 | **Despliegue sin claves** (P15) | Muestran la relación de confianza del rol OIDC. Ejecutan el workflow de despliegue **desde otra rama** (*Run workflow*): AWS rechaza la credencial. Desde `main` funciona. | La condición `ref:refs/heads/main` del rol; el fallo desde la otra rama y el éxito desde `main`. | Credenciales temporales; por qué no hay Access Key; qué aporta CloudFormation como infraestructura como código. |
| 10 | **Búsqueda semántica y P99** (P18) | Prueba de carga con k6 sobre el buscador (`/?q=…`) con varias búsquedas. | Promedio, p95 y p99 del ALB en el dashboard. | Qué hace el índice HNSW; qué significa que solo el P99 esté alto y a qué parte de los usuarios afecta. |

## 📚 Glosario: servicios, siglas y términos del laboratorio

Las mismas definiciones del balotario y del examen. Si un término no está aquí, está en la explicación de alguna de las 50 preguntas del balotario.

#### Servicios de AWS

| Servicio | Para qué sirve | Ejemplo en URPOnline |
|---|---|---|
| **Amazon VPC** | Red privada y aislada en AWS donde se ubican los recursos, dividida en subredes. | VPC 10.0.0.0/16 de URPOnline con subredes públicas, de aplicación y de datos. |
| **Internet Gateway (IGW)** | Conecta la VPC con Internet; lo usan las subredes públicas. | El ALB y los NAT Gateway están en subredes con ruta al IGW. |
| **NAT Gateway** | Da salida a Internet a recursos de subredes privadas, para conexiones iniciadas desde dentro. | Las EC2 sin IP pública instalan paquetes con dnf a través del NAT de su zona. |
| **Tabla de rutas** | Indica a dónde se envía el tráfico de una subred según su destino. | La subred de aplicación envía 0.0.0.0/0 al NAT Gateway. |
| **Amazon EC2** | Servidores virtuales (instancias) en la nube. | Cada EC2 corre Nginx y la aplicación ASP.NET Core. |
| **Amazon EC2 Auto Scaling** | Mantiene la cantidad deseada de EC2 y las reemplaza o aumenta automáticamente. | Mantiene 2 EC2, una en AZ-a y otra en AZ-b. |
| **Launch Template** | Plantilla con la configuración para lanzar EC2 (AMI, tipo, rol IAM, Security Group, User Data). | Auto Scaling crea cada EC2 de URPOnline a partir de esta plantilla. |
| **Application Load Balancer (ALB) y Target Group** | Reparte el tráfico HTTP/HTTPS entre las instancias registradas en un Target Group, según su health check. | Reparte las visitas de la tienda entre las dos EC2. |
| **Amazon CloudFront** | Red de distribución de contenido (CDN) que entrega el sitio y puede dar HTTPS. | Delante del ALB, da la dirección https://…cloudfront.net. |
| **AWS Certificate Manager (ACM)** | Emite y renueva certificados para HTTPS. | Certificado para un dominio propio de la tienda. |
| **Amazon Route 53** | Servicio de DNS de AWS (registros como CNAME). | Componente en evaluación, aún no desplegado. |
| **AWS WAF y AWS Shield** | WAF filtra peticiones web; Shield protege contra ataques de denegación de servicio. | Componentes en evaluación para proteger la tienda. |
| **Amazon RDS (PostgreSQL)** | Base de datos relacional administrada por AWS. | Guarda pedidos, stock y pagos; con pgvector, la búsqueda semántica. |
| **pgvector (extensión de PostgreSQL en Amazon RDS)** | Permite guardar vectores en PostgreSQL y buscar los más parecidos; es la base de la búsqueda semántica. | La tabla ProductEmbeddings guarda el vector de cada producto. |
| **Amazon ElastiCache (Redis)** | Almacén en memoria administrado, muy rápido. | Guarda la sesión y el carrito compartidos entre las EC2. |
| **Amazon S3** | Almacenamiento de objetos (archivos). | Guarda el ZIP de cada versión de la aplicación y el archivo del catálogo. |
| **VPC Endpoint (Gateway e Interface)** | Acceso privado desde la VPC a servicios de AWS sin pasar por Internet. | Las EC2 llegan a S3 por la red de AWS. |
| **AWS Secrets Manager** | Guarda secretos cifrados (contraseñas, tokens) y los entrega a quien tiene permiso. | Guarda la contraseña de RDS. |
| **AWS IAM** | Controla quién puede hacer qué en AWS mediante usuarios, roles y políticas. | El rol de las EC2 les da permiso solo sobre lo que usan. |
| **AWS Systems Manager Session Manager** | Abre una terminal en una EC2 desde la consola o la CLI, autenticada con IAM. | Entrar a una EC2 privada para revisar el servicio de la tienda. |
| **Amazon CloudWatch** | Métricas, logs, alarmas y dashboards de los recursos y de la aplicación. | Dashboard con pedidos, ventas, CPU y errores de la tienda. |
| **VPC Flow Logs** | Registra las conexiones de red de la VPC (origen, destino, puerto, aceptada o rechazada). | Ver las conexiones hacia la subred de datos. |
| **VPC Reachability Analyzer** | Analiza la configuración de red entre un origen y un destino. | Analizar el camino entre Internet y RDS. |
| **AWS CloudTrail** | Registra las acciones realizadas en la cuenta de AWS. | Auditar cambios en la cuenta del laboratorio. |
| **Amazon SNS** | Envía notificaciones (correo, SMS) a los suscriptores de un tema. | Avisar al equipo por correo. |
| **AWS CloudFormation** | Crea y actualiza la infraestructura a partir de una plantilla (YAML). | La plantilla que crea toda la infraestructura de URPOnline. |
| **AWS SDK** | Librerías para que una aplicación use servicios de AWS desde su código. | La tienda en C# lee secretos y publica métricas. |
| **AWS Cost Explorer, AWS Budgets y Cost Anomaly Detection** | Ver el gasto por servicio, avisar al pasar un monto y detectar gastos inusuales. | Controlar el costo del laboratorio. |

#### Siglas y términos

| Sigla o término | Qué significa | Ejemplo en URPOnline |
|---|---|---|
| **AZ (zona de disponibilidad)** | Centro de datos independiente dentro de una región de AWS. | AZ-a y AZ-b en Ohio (us-east-2). |
| **10.0.0.0/16 (CIDR)** | Notación de un rango de direcciones IP. | Rango de la VPC; 10.0.11.0/24 es una subred de aplicación. |
| **0.0.0.0/0** | Ruta por defecto: todas las direcciones IPv4. | Ruta de salida de la subred de aplicación. |
| **Subred pública / privada** | Una tiene ruta al Internet Gateway; la otra no. | El ALB está en una pública; las EC2, en una privada. |
| **IP pública / privada** | Dirección alcanzable desde Internet / solo dentro de la VPC. | Las EC2 de URPOnline solo tienen IP privada. |
| **SG (Security Group)** | Firewall que se asocia a un recurso (EC2, RDS, ALB). | El SG de RDS controla quién llega al puerto 5432. |
| **NACL** | Lista de control de acceso de red, asociada a una subred. | La NACL de la subred de datos. |
| **Stateful / stateless** | Recuerda las conexiones y deja pasar la respuesta / no las recuerda. | Se usa para comparar SG y NACL. |
| **Puertos efímeros (1024-65535)** | Puertos temporales que usa el cliente para recibir respuestas. | Puertos de respuesta de una conexión a la base. |
| **Puertos 80, 443, 5432, 6379, 8080** | HTTP, HTTPS, PostgreSQL, Redis y el puerto interno de la aplicación. | Nginx en 80, RDS en 5432, Redis en 6379, Kestrel en 8080. |
| **HTTP / HTTPS / TLS** | Protocolo web / web cifrado con TLS (certificado). | La tienda se abre con https:// en CloudFront. |
| **DNS / CNAME** | Sistema que traduce nombres a direcciones / registro que apunta un nombre a otro. | Registros de Route 53. |
| **Nginx / proxy inverso** | Servidor que recibe las peticiones y las reenvía a la aplicación. | Nginx en el 80 reenvía a 127.0.0.1:8080. |
| **Kestrel / ASP.NET Core / .NET 8** | Servidor web de .NET / framework web / plataforma de la aplicación. | La tienda URPOnline. |
| **127.0.0.1** | Dirección de la propia máquina (solo accesible desde dentro). | Kestrel escucha en 127.0.0.1:8080. |
| **Health check / healthy / unhealthy** | Prueba periódica de salud de una instancia / sana / no sana. | El Target Group revisa la ruta /health. |
| **502 Bad Gateway** | Error HTTP: el proxy no pudo comunicarse con el servidor de atrás. | Error que devuelve Nginx. |
| **Sticky sessions** | El balanceador envía a un cliente siempre a la misma instancia. | Opción del ALB. |
| **Sesión / cookie** | Datos de la visita de un cliente / dato que el navegador guarda y envía al sitio. | La sesión guarda el carrito. |
| **Caché / TTL / Cache-Aside** | Copia rápida de datos / tiempo de vida de una entrada / patrón de uso de la caché por la aplicación. | Precios guardados en Redis. |
| **Transacción / actualización atómica** | Operación que se hace completa o no se hace / sin interferencia de otra a la vez. | Descontar el stock al confirmar un pedido. |
| **Idempotencia** | Repetir una operación da el mismo resultado que hacerla una vez. | Un doble clic en «Confirmar pedido». |
| **Multi-AZ / failover** | Copia en otra zona / cambio automático a la copia si falla la principal. | Opción de RDS. |
| **PITR (point-in-time recovery)** | Restaurar la base a un momento exacto dentro del período de retención. | Recuperar la base a un minuto anterior. |
| **pgvector** | Extensión de PostgreSQL para guardar vectores y buscar los más parecidos. | Búsqueda semántica del catálogo de URPOnline. |
| **Vector (embedding)** | Lista de números que representa el significado de un texto. | Cada producto tiene un vector de 384 números. |
| **Búsqueda semántica** | Buscar por significado y no por palabras exactas. | «Algo para escuchar música» encuentra los audífonos. |
| **HNSW** | Tipo de índice de pgvector para encontrar vecinos cercanos. | Índice de la tabla de vectores de los productos. |
| **<=> (distancia coseno)** | Operador de pgvector que mide qué tan distintos son dos vectores (similitud = 1 − distancia). | Ordenar los productos del más al menos parecido. |
| **Percentil P95 / P99** | Valor bajo el cual queda el 95 % / 99 % de las mediciones. | Latencia del ALB en el dashboard. |
| **IaC (infraestructura como código)** | Describir la infraestructura en archivos versionados. | La plantilla de CloudFormation. |
| **YAML** | Formato de texto para describir configuraciones. | La plantilla de CloudFormation está en YAML. |
| **User Data / cloud-init** | Script que corre al arrancar una EC2 / herramienta que lo ejecuta y deja su log. | /var/log/cloud-init-output.log. |
| **cfn-signal** | Aviso de la EC2 a CloudFormation de que arrancó bien o mal. | Confirma que /health responde. |
| **Rolling update** | Reemplazar las instancias de a poco, sin detener el servicio. | Publicar una versión nueva. |
| **ArtifactKey** | Parámetro de la plantilla con la ubicación del ZIP de la versión en S3. | releases/urponline-v1.zip. |
| **IAM role / Access Key** | Permisos temporales que asume un recurso / clave permanente de un usuario. | Las EC2 usan un rol, no Access Keys. |
| **OIDC (OpenID Connect)** | Estándar para demostrar identidad sin compartir contraseñas. | GitHub Actions entra a AWS sin claves. |
| **GitHub Actions** | Servicio de GitHub que ejecuta tareas automáticas (compilar, desplegar). | Despliega la tienda en AWS. |
| **Root (usuario raíz)** | Usuario dueño de la cuenta de AWS, con todos los permisos. | Nunca se usan sus claves. |
| **dnf** | Gestor de paquetes de Amazon Linux. | Instalar parches en las EC2. |
| **PutMetricData / namespace** | Llamada del SDK para publicar una métrica / grupo de métricas. | Métricas de pedidos en el namespace URPOnline. |
| **PgBouncer** | Administrador de conexiones (pool) entre la aplicación y PostgreSQL: muchas conexiones de la aplicación comparten pocas conexiones reales. | Evita que RDS rechace conexiones cuando Auto Scaling agrega EC2. |
| **Snapshot** | Copia de un disco o de una base en un momento. | Snapshot final de RDS al borrar la pila. |

#### Otros términos que aparecen en los casos

| Término | Qué significa | Ejemplo en URPOnline |
|---|---|---|
| **Región / zona de disponibilidad** | Región: zona geográfica de AWS con varios centros de datos; zona de disponibilidad (AZ): centro de datos independiente dentro de la región. | URPOnline corre en la región Ohio (us-east-2), con una EC2 en AZ-a y otra en AZ-b. |
| **Sistema operativo / parche** | Software base de la EC2 (Amazon Linux) / actualización que corrige un error o una vulnerabilidad. | Instalar un parche crítico con dnf en las dos EC2. |
| **Escuchar en un puerto: 127.0.0.1 frente a 0.0.0.0** | Un programa «escucha» en una dirección y un puerto. 127.0.0.1 solo acepta conexiones de la misma máquina; 0.0.0.0 acepta desde cualquier red. | Kestrel escucha en 127.0.0.1:8080 (solo Nginx le habla); escuchar en 0.0.0.0:8080 y quitar Nginx lo expondría. |
| **Firma del servidor** | Nombre del programa que generó una respuesta HTTP (encabezado Server o pie de la página de error). | Un «502 Bad Gateway» con la firma de Nginx indica que Nginx responde pero Kestrel no. |
| **http:// y https://** | Comienzo de una dirección web: http:// viaja sin cifrar; https:// viaja cifrado con TLS y el navegador lo muestra como seguro. | Solo por http:// el navegador marca la tienda como «No seguro». |
| **Bucket** | Contenedor de archivos (objetos) en Amazon S3; se le aplican políticas y versionado. | El bucket de artefactos guarda releases/urponline-v1.zip. |
| **ZIP de la versión (artefacto)** | Archivo comprimido con la aplicación compilada; su ubicación en S3 se pasa a la plantilla. | ArtifactKey = releases/urponline-v1.zip. |
| **Rama main / push** | Rama principal del repositorio en GitHub / enviar los commits locales al repositorio remoto. | Un push a main dispara el despliegue en GitHub Actions. |
| **Política (de seguridad / de IAM)** | Regla que dice qué está permitido: como norma de la empresa, o como documento de IAM que da permisos. | La política de seguridad prohíbe abrir puertos de entrada desde Internet. |
| **Evento (de CloudFormation)** | Registro de cada paso de una pila (creación, actualización, error, reversión) visible en la consola. | El evento de reversión de la actualización del sábado. |
| **Denegación de servicio (DoS / DDoS)** | Ataque que satura un servicio con peticiones para que deje de atender a los usuarios reales. | AWS Shield protege la tienda contra estos ataques. |
| **Embedding** | Vector de números que resume el significado de un texto; textos parecidos tienen vectores cercanos. | Cada producto tiene un embedding de 384 números en ProductEmbeddings. |
| **Modelo de embeddings (paraphrase-multilingual-MiniLM-L12-v2)** | Modelo de lenguaje pequeño (MiniLM, 12 capas) que convierte un texto, en más de 50 idiomas, en un vector de 384 números (embedding). | Corre dentro de cada EC2 con ONNX Runtime, sin API externa: tokeniza, promedia los vectores de los tokens y normaliza. |
| **Similitud coseno** | 1 − distancia coseno: cuanto más cerca de 1, más parecidos son dos vectores. | La tienda solo sugiere productos con similitud mayor o igual a un mínimo. |
| **Saturación de conexiones / pool de conexiones** | PostgreSQL admite un máximo de conexiones; si cada EC2 abre las suyas y Auto Scaling agrega instancias, se supera y rechaza nuevas. Un pool reúne muchas conexiones de la aplicación en pocas conexiones reales que se reutilizan. | PgBouncer (puerto 6432) entre las EC2 y RDS (puerto 5432). |
| **CloudWatch Metrics** | Parte de CloudWatch que guarda métricas numéricas (CPU, solicitudes, pedidos); no registra quién hizo un cambio. | Quién cambió un Security Group se ve en CloudTrail, no en Metrics. |
| **Base de datos relacional** | Guarda datos en tablas relacionadas y garantiza transacciones. | PostgreSQL en RDS guarda pedidos, stock y pagos. |


## 🚀 Descargar y correr la tienda en tu equipo

Requisitos: **.NET 8 SDK**, **Docker Desktop** abierto y **Git**. Cómo instalarlos: [CONFIGURACION_VSCODE.md](CONFIGURACION_VSCODE.md).

```powershell
# 1. Descargar el proyecto
git clone https://github.com/cortexgovernor/tiendaonline.git
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

#### 📐 Teoría: búsqueda semántica con índice HNSW y distancia coseno

**1. Qué es un índice.** Una estructura adicional que la base de datos mantiene para no recorrer toda la tabla en cada búsqueda, como el índice de un libro. El índice normal de PostgreSQL (**B-tree**) ordena valores y sirve para un valor exacto o su comienzo (`WHERE "Name" = 'Cámara X'`, `LIKE 'Cám%'`). Por eso, con B-tree, el cliente tiene que escribir el nombre **como está guardado**: «tomar fotos» nunca encuentra «Cámara X».

**2. Embedding.** Cada producto se convierte en un **vector de 384 números** que resume su significado (tabla `ProductEmbeddings`, columna `vector(384)`). Lo que el cliente escribe también se convierte en un vector. Textos con significado parecido producen vectores que **apuntan en direcciones parecidas**, aunque no compartan palabras.

**2b. Qué modelo convierte el texto en embedding.** La tienda usa **`paraphrase-multilingual-MiniLM-L12-v2`** (de *sentence-transformers*, publicado en Hugging Face):

| | |
|---|---|
| Qué es | Un modelo de lenguaje **pequeño** de la familia MiniLM (12 capas, ~118 millones de parámetros), entrenado para que frases con el **mismo significado** —en más de 50 idiomas, incluido el español— produzcan vectores cercanos. |
| Qué hace | Convierte cada texto (nombre, categoría y descripción del producto, o lo que escribe el cliente) en un vector de 384 números: su embedding. |
| Dónde corre | **Dentro de cada EC2**, con **ONNX Runtime** (versión cuantizada `model_quint8_avx2.onnx`): no llama a ninguna API externa, no tiene costo por consulta y el texto del cliente no sale del servidor. |
| Cómo se instala | `scripts/descargar-modelo.ps1` lo descarga en `ml-models/` desde una versión fijada y verifica su **SHA-256**; `dotnet publish` lo incluye en el ZIP que va a S3. |

Pasos en `Services/EmbeddingService.cs`:

1. **Tokenizar** el texto con SentencePiece (como máximo 128 tokens).
2. El modelo devuelve **un vector de 384 por cada token**.
3. **Promediarlos** (*mean pooling*) para obtener un solo vector de 384 para todo el texto.
4. **Normalizar** el vector a longitud 1 (L2). Así, la similitud coseno es simplemente el producto punto.
5. Guardar el vector en `ProductEmbeddings` (al cargar productos) o compararlo con `<=>` (al buscar).

**3. Distancia coseno: la métrica que gradúa la proximidad.** Compara la **dirección** de dos vectores, no su tamaño:

```
similitud coseno = cos θ = (A · B) / (|A| × |B|)        → 1 = misma dirección, 0 = sin relación, −1 = opuestos
distancia coseno  = 1 − similitud                        → 0 = mismo significado, 1 = sin relación, 2 = opuestos
```

Ejemplo con vectores de 2 números (los reales tienen 384): A = (1, 0), B = (0.9, 0.1), C = (0, 1).
A y B casi apuntan igual → distancia ≈ 0.006 (muy parecidos); A y C forman 90° → distancia = 1 (sin relación).
En pgvector la distancia coseno es el operador **`<=>`**: se ordena de menor a mayor para obtener primero los más cercanos, y un **umbral** decide qué tan cerca debe estar un producto para mostrarlo.

**4. Índice HNSW: cómo encuentra rápido a los más cercanos.** Sin índice, la búsqueda compara el vector de la consulta contra **todos** los productos. **HNSW** (*Hierarchical Navigable Small World*) arma un **grafo por capas**:

- Cada producto es un nodo conectado con sus vecinos más parecidos.
- La capa de arriba tiene pocos nodos con enlaces «largos» (como autopistas); cada capa de abajo tiene más nodos y enlaces más cortos; la capa 0 los tiene a todos.
- Para buscar, entra por arriba, salta siempre al vecino más cercano a la consulta, baja de capa y repite, hasta llegar a los **vecinos más cercanos** en pocos pasos.
- Es **aproximado**: puede omitir algún vecino muy cercano, a cambio de responder en milisegundos aunque haya millones de vectores.

Por eso la tienda puede **sugerir productos mientras el cliente escribe**, sin que termine el nombre completo. El índice se crea con `vector_cosine_ops` para que acelere justamente las consultas ordenadas por `<=>`:

```sql
-- Migración BusquedaVectorialPgvector
CREATE INDEX "IX_ProductEmbeddings_Embedding_hnsw"
    ON "ProductEmbeddings" USING hnsw ("Embedding" vector_cosine_ops);

-- Services/ProductVectorStore.cs
SELECT "ProductId", 1 - ("Embedding" <=> @q::vector) AS similarity   -- similitud = 1 − distancia coseno
FROM "ProductEmbeddings"
WHERE NOT ("ProductId" = ANY(@exclude)) AND 1 - ("Embedding" <=> @q::vector) >= @min   -- umbral de proximidad
ORDER BY "Embedding" <=> @q::vector                                                    -- los más cercanos primero (usa HNSW)
LIMIT @limit;
```

Subir `@min` exige más similitud: aparecen **menos** productos, pero **más cercanos** a lo buscado; bajarlo muestra más, pero menos relacionados.

#### 🔌 Teoría: PgBouncer y la saturación de conexiones

- **El problema.** PostgreSQL crea **un proceso por cada conexión** y admite un **máximo** de conexiones a la vez (`max_connections`, que en RDS depende del tamaño de la instancia). Cada EC2 abre su propio grupo de conexiones; cuando Auto Scaling agrega instancias, la suma crece (8 EC2 × 20 conexiones = 160) y, al pasar el máximo, PostgreSQL **rechaza las nuevas** (`too many clients`) aunque su CPU esté baja. Eso es **saturación de conexiones**.
- **La solución: un pool de conexiones.** **PgBouncer** se ubica entre las EC2 y RDS. La aplicación se conecta a PgBouncer (**puerto 6432**) y PgBouncer mantiene **pocas conexiones reales** a PostgreSQL (**puerto 5432**), que presta a cada consulta y recupera al terminar. Muchas conexiones de la aplicación comparten un grupo pequeño de conexiones reales.
- **Lo que NO hace.** No guarda datos (la sesión y el carrito van en **Redis**; pedidos, stock y pagos en **PostgreSQL**), no reemplaza a Secrets Manager y no replica la base (eso es **Multi-AZ**).
- **En URPOnline.** La plantilla base conecta la aplicación **directo a RDS por 5432**. PgBouncer es un **componente objetivo, todavía no desplegado**: la topología avanzada usa 6432. No apuntes la cadena de conexión a 6432 mientras PgBouncer no exista.

```
Topología base:      EC2 (app) ──5432──► RDS PostgreSQL
Topología avanzada:  EC2 (app) ──6432──► PgBouncer (pool) ──5432──► RDS PostgreSQL
```

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
