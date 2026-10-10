using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using TiendaOnline.Data;
namespace TiendaOnline.Services;

// Calcula el embedding de cada producto nuevo o modificado (al arrancar y luego cada N minutos),
// para que los productos que carga el Worker también aparezcan en la búsqueda semántica.
public class ProductEmbeddingIndexer(IServiceScopeFactory scopes, EmbeddingService embeddings,
  IConfiguration config, ILogger<ProductEmbeddingIndexer> log) : BackgroundService {
 protected override async Task ExecuteAsync(CancellationToken ct) {
  if (!embeddings.IsAvailable) return;
  var interval = TimeSpan.FromMinutes(config.GetValue("Embeddings:RefreshMinutes", 10));
  while (!ct.IsCancellationRequested) {
   try { await IndexAsync(ct); }
   catch (Exception ex) when (ex is not OperationCanceledException) { log.LogError(ex, "Error indexando embeddings de productos"); }
   await Task.Delay(interval, ct);
  }
 }

 public static string SourceText(Models.Product p) => $"{p.Name}. {p.Category}. {p.Description}";

 private async Task IndexAsync(CancellationToken ct) {
  using var scope = scopes.CreateScope();
  var db = scope.ServiceProvider.GetRequiredService<ShopDbContext>();
  var store = scope.ServiceProvider.GetRequiredService<ProductVectorStore>();
  if (!await store.EnsureSchemaAsync(ct)) { log.LogWarning("La tabla Products no existe todavía; se reintenta luego."); return; }
  var existing = await store.GetHashesAsync(ct);
  var products = await db.Products.AsNoTracking().ToListAsync(ct);
  int updated = 0;
  foreach (var p in products) {
   var text = SourceText(p);
   var hash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(text)));
   if (existing.TryGetValue(p.Id, out var old) && old == hash) continue;
   await store.UpsertAsync(p.Id, embeddings.Embed(text), hash, ct);
   updated++;
  }
  if (updated > 0) log.LogInformation("Embeddings actualizados: {Count} producto(s)", updated);
 }
}
