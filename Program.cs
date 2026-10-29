using Microsoft.EntityFrameworkCore;using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.DataProtection;
using Amazon.S3;
using Amazon.SecretsManager;
using Amazon.CloudWatch;
using TiendaOnline.Data;
using TiendaOnline.Services;
var builder=WebApplication.CreateBuilder(args);
builder.Services.AddControllersWithViews();
builder.Services.AddHttpContextAccessor();
var db=builder.Configuration.GetConnectionString("Postgres")?? throw new InvalidOperationException("Configure ConnectionStrings:Postgres; apunte a PgBouncer 6432 en AWS.");
db=DatabaseSecret.BuildConnectionString(builder.Configuration,db);
builder.Services.AddDbContext<ShopDbContext>(o=>o.UseNpgsql(db));
// Migraciones: conexión directa a RDS (5432). pg_advisory_lock necesita una conexión fija, que PgBouncer en modo transaction no garantiza.
// Si ConnectionStrings:PostgresMigrations no existe (p. ej. en tu PC, sin PgBouncer), se usa la misma cadena de la app.
var migrationsDb=builder.Configuration.GetConnectionString("PostgresMigrations");
migrationsDb=string.IsNullOrWhiteSpace(migrationsDb)?db:DatabaseSecret.BuildConnectionString(builder.Configuration,migrationsDb);
var redis=builder.Configuration.GetConnectionString("Redis");
if(!string.IsNullOrWhiteSpace(redis)) builder.Services.AddStackExchangeRedisCache(o=>o.Configuration=redis);
else if(builder.Environment.IsDevelopment()) builder.Services.AddDistributedMemoryCache();
else throw new InvalidOperationException("Redis es obligatorio fuera de Development para compartir sesiones entre EC2.");
// Claves que cifran la cookie de sesión: en Redis (ElastiCache) para que todas las EC2 detrás del ALB las compartan.
var dataProtection=builder.Services.AddDataProtection().SetApplicationName("URPOnline");
if(!string.IsNullOrWhiteSpace(redis))
 dataProtection.PersistKeysToStackExchangeRedis(StackExchange.Redis.ConnectionMultiplexer.Connect(redis),"URPOnline-DataProtection-Keys");
builder.Services.AddSession(o=>{
 o.Cookie.Name=".URPOnline.Session";o.Cookie.HttpOnly=true;
 // SameAsRequest: la cookie es Secure cuando el visitante entra por HTTPS (ALB con ACM o CloudFront); en el laboratorio el ALB usa HTTP.
 o.Cookie.SecurePolicy=CookieSecurePolicy.SameAsRequest;o.Cookie.SameSite=SameSiteMode.Lax;
 o.Cookie.IsEssential=true;o.IdleTimeout=TimeSpan.FromMinutes(30);
});
builder.Services.AddScoped<CartService>();builder.Services.AddScoped<BusinessEvents>();
builder.Services.AddScoped<BusinessMetrics>();
// ActiveCarts: personas distintas con carrito en los últimos 5 min, contadas en Redis (compartido por las EC2).
builder.Services.AddSingleton<ActiveCartsTracker>();
builder.Services.AddHostedService<ActiveCartsPublisher>();
// Búsqueda semántica: modelo local ONNX + pgvector en PostgreSQL.
builder.Services.AddSingleton<EmbeddingService>();
builder.Services.AddScoped<ProductVectorStore>();
builder.Services.AddHostedService<ProductEmbeddingIndexer>();
builder.Services.AddDefaultAWSOptions(builder.Configuration.GetAWSOptions());
builder.Services.AddAWSService<IAmazonS3>();
builder.Services.AddAWSService<IAmazonSecretsManager>();
builder.Services.AddAWSService<IAmazonCloudWatch>();
var app=builder.Build();
if(!app.Environment.IsDevelopment()){app.UseExceptionHandler("/Home/Error");app.UseHsts();}
app.UseForwardedHeaders(new ForwardedHeadersOptions {ForwardedHeaders=ForwardedHeaders.XForwardedFor|ForwardedHeaders.XForwardedProto,KnownNetworks={ },KnownProxies={ }});
app.UseStaticFiles();app.UseRouting();app.UseSession();
app.MapGet("/health",()=>Results.Ok(new{status="ok"}));
app.MapControllerRoute("default","{controller=Home}/{action=Index}/{id?}");
if(app.Configuration.GetValue<bool>("InitializeDatabase")){
 // Contexto propio con la conexión directa a RDS (5432): el lock, la migración y el seed comparten la misma conexión fija.
 await using var database=new ShopDbContext(new DbContextOptionsBuilder<ShopDbContext>().UseNpgsql(migrationsDb).Options);
 // Migraciones EF Core versionadas (Data/Migrations): tablas + pgvector + índice HNSW. Luego datos de ejemplo.
 // pg_advisory_lock: si varias EC2 arrancan a la vez, solo una migra; las demás esperan y encuentran todo aplicado.
 await database.Database.OpenConnectionAsync();
 try {
  await database.Database.ExecuteSqlRawAsync("SELECT pg_advisory_lock(727001)");
  await database.Database.MigrateAsync();await SeedData.LoadAsync(database);
 } finally {
  await database.Database.ExecuteSqlRawAsync("SELECT pg_advisory_unlock(727001)");
  await database.Database.CloseConnectionAsync();
 }
}
app.Run();