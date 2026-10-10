# URPOnline — guía de CloudFormation base (importante leer antes)

## Alcance real
La plantilla `urponline-oneclick.yaml` crea **infraestructura base y arranca un artefacto .NET precompilado** en las EC2 privadas. No implementa todas las capacidades descritas en `SPEC.md`. El nombre «oneclick» significa **una ejecución de CloudFormation tras preparar el ZIP y bucket**, no un único clic desde código fuente, ni garantía de producción. Es una plantilla para laboratorio; debe endurecerse antes de exponerse públicamente.

## Preparar el artefacto desde el proyecto v2
Requisitos locales: .NET SDK 8, AWS CLI configurada (`aws configure sso` o `aws configure` usando credenciales temporales), permiso `s3:PutObject`, bucket S3 existente en la **misma región**.

```bash
unzip URPOnline_v2_CSharp_AWS_SDK_Proyecto.zip
cd TiendaOnline_NET_AWS
dotnet restore
dotnet build -c Release
# No continuar si falla: corregir errores con Claude Code y ejecutar pruebas.
dotnet publish TiendaOnline.csproj -c Release -o out
(cd out && zip -r ../urponline-publish.zip .)
# Comprueba el ensamblado principal ANTES de subir el ZIP:
unzip -l urponline-publish.zip | grep 'TiendaOnline.dll'
aws sts get-caller-identity
aws s3 cp urponline-publish.zip s3://MI_BUCKET/releases/urponline-publish.zip
```

Si `dotnet publish` falla o el nombre ensamblado no es `TiendaOnline.dll`, **ajustar y probar** antes de desplegar. El YAML no compila C#: instala solo runtime .NET 8.

## En AWS Console
1. Elegir una región con dos AZ y recursos RDS/ElastiCache disponibles.
2. Crear bucket privado S3 para artefactos. Bloquear acceso público. Subir `releases/urponline-publish.zip` desde consola o CLI.
3. CloudFormation → **Create stack** → With new resources → **Upload a template file** → seleccionar `urponline-oneclick.yaml`.
4. Nombre `URPOnlineLab`; completar `ArtifactBucket`, `ArtifactKey`, `DBPassword` (contraseña segura), tipo EC2/RDS, `EnableMultiAZ=true`.
5. Reconocer creación IAM (`CAPABILITY_IAM`) y revisar costes. Crear stack y esperar `CREATE_COMPLETE`.
6. Revisar Outputs ALBDNS / RDSEndpoint / RedisPrimary / SupplierBucket. EC2→ Auto Scaling → instancias; Target groups → **Health checks**. RDS/Redis no tienen acceso público.
7. Probar `http://ALBDNS/health` **solo en laboratorio**; antes de usar credenciales reales o datos personales configurar HTTPS 443 + ACM + dominio, WAF, Secrets Manager, auth y Data Protection distribuida.
8. Revisar logs con SSM Session Manager (`sudo journalctl -u urponline -n 100`) y `sudo nginx -t` si target unhealthy.

## Advertencias que hay que resolver
- **El YAML actual almacena DBPassword dentro de EC2 `/etc/urponline.env`** durante bootstrap y su valor pasa por CloudFormation `NoEcho`; producción: migrar a Secrets Manager/SSM SecureString con IAM, evitar exponer secretos en UserData.
- **No ejecuta migraciones** ni siembra tablas. La app v2 usa `EnsureCreated` bajo `InitializeDatabase`; para un entorno consistente: crear un job controlado de migraciones y semilla antes de activar tráfico. Una pila puede estar creada con aplicación `Unhealthy` hasta completar DB.
- Redis exige TLS; validar conexión y trust chain. El anillo de claves ASP.NET Data Protection **no está compartido** en la app v2, y eso puede romper cookies tras cambio de EC2. Implementar F02 antes del test multi-instancia.
- El YAML base no despliega PgBouncer 6432; la app se conecta temporalmente a RDS 5432. PgBouncer HA, WAF, SSL 443, SecurityHub, X-Ray/ADOT, CloudWatch dashboards, sistema de feature flags completo, Worker y pagos de proveedor son fases posteriores.
- SG de ALB expone 80 HTTP; HTTPS obligatorio fuera del laboratorio. EC2 no tiene SSH entrante; administrar por SSM.
- Los NAT Gateways, RDS, Redis, EC2 y ALB tienen **coste recurrente**. Para eliminar: CloudFormation → Delete stack, revisar snapshots retenidas por `DeletionPolicy: Snapshot`, bucket con objetos y recursos huérfanos. RDS snapshots retienen costes.
- En Amazon Linux 2023 los nombres/versiones de paquetes pueden variar por AMI/región. Probar UserData con logs `/var/log/cloud-init-output.log`; ajustar repositorio de .NET si runtime no está disponible.
- La plantilla describe la fase base **no avalada con una ejecución AWS real**. Ejecutar `cfn-lint` y `aws cloudformation validate-template` con credenciales antes de crear recursos.

## Diagnóstico rápido
| Síntoma | Comprobar |
|---|---|
| Target `Unhealthy` | `journalctl -u urponline`; Nginx puerto 80; backend 8080; `/health`; runtime |
| Nginx 502 | Kestrel no escucha `127.0.0.1:8080`; servicio caído; rutas ProxyPass |
| PostgreSQL falla | SG 5432, RDS endpoint, BD/migraciones, SSL, secreto, NAT solo para bootstrap |
| Redis falla | SG 6379, TLS `ssl=true`, Redis Primary endpoint, DNS interno |
| S3 403 | IAM Instance Profile y políticas `GetObject` del bucket de artefactos |
| Precio incorrecto | Validaciones server-side, snapshot de precio y moneda, caché flags |

## Evolución a un verdadero despliegue automatizado y seguro
En Claude Code, pedir: *«Implementa F01–F10 de SPEC.md; añade migrations job, secret bootstrap con Secrets Manager, anillo Key Ring distribuido, ACM 443, WAF, PgBouncer HA, CloudWatch/ADOT, Worker dedicado, tests de integración, y pipeline CI/CD que haga build+publicación S3+CloudFormation Change Set. No declares producción hasta demostrar pruebas.»*
