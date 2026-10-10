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
builder.Services.AddDbContext<ShopDbContext>(o=>o.UseNpgsql(db));
var redis=builder.Configuration.GetConnectionString("Redis");
if(!string.IsNullOrWhiteSpace(redis)) builder.Services.AddStackExchangeRedisCache(o=>o.Configuration=redis);
else if(builder.Environment.IsDevelopment()) builder.Services.AddDistributedMemoryCache();
else throw new InvalidOperationException("Redis es obligatorio fuera de Development para compartir sesiones entre EC2.");
builder.Services.AddDataProtection().SetApplicationName("URPOnline");
builder.Services.AddSession(o=>{
 o.Cookie.Name=".URPOnline.Session";o.Cookie.HttpOnly=true;
 o.Cookie.SecurePolicy=builder.Environment.IsDevelopment()?CookieSecurePolicy.SameAsRequest:CookieSecurePolicy.Always;o.Cookie.SameSite=SameSiteMode.Lax;
 o.Cookie.IsEssential=true;o.IdleTimeout=TimeSpan.FromMinutes(30);
});
builder.Services.AddScoped<CartService>();builder.Services.AddScoped<BusinessEvents>();
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
 using var scope=app.Services.CreateScope();var database=scope.ServiceProvider.GetRequiredService<ShopDbContext>();
 // Migraciones EF Core versionadas (Data/Migrations): tablas + pgvector + índice HNSW. Luego datos de ejemplo.
 await database.Database.MigrateAsync();await SeedData.LoadAsync(database);
}
app.Run();
