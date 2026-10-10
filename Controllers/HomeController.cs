using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TiendaOnline.Data;
using TiendaOnline.Models;
using TiendaOnline.Services;
namespace TiendaOnline.Controllers;
public class HomeController(ShopDbContext db,BusinessEvents events,EmbeddingService embeddings,ProductVectorStore vectors,
  IConfiguration config,ILogger<HomeController> log) : Controller {
 public async Task<IActionResult> Index(string? category,string? q,CancellationToken ct) {
  var products=db.Products.AsNoTracking().AsQueryable();
  if(!string.IsNullOrWhiteSpace(category)) products=products.Where(x=>x.Category==category);
  if(!string.IsNullOrWhiteSpace(q)) products=products.Where(x=>x.Name.ToLower().Contains(q.ToLower()));
  var results=await products.OrderBy(x=>x.Id).ToListAsync(ct);
  ViewBag.Query=q; ViewBag.Category=category;
  ViewBag.Related=new List<RelatedProduct>();
  ViewBag.SemanticUnavailable=!string.IsNullOrWhiteSpace(q) && !embeddings.IsAvailable ? embeddings.UnavailableReason : null;
  if(!string.IsNullOrWhiteSpace(q) && embeddings.IsAvailable) ViewBag.Related=await RelatedAsync(q,results,ct);
  await events.RecordAsync("visit");
  return View(results);
 }

 // Productos semánticamente parecidos a la búsqueda (pgvector), excluyendo los que ya coincidieron por texto.
 private async Task<List<RelatedProduct>> RelatedAsync(string q,List<Product> direct,CancellationToken ct) {
  try {
   if(!await vectors.EnsureSchemaAsync(ct)) return new();
   var hits=await vectors.SearchAsync(embeddings.Embed(q),
     config.GetValue("Embeddings:RelatedLimit",4),config.GetValue("Embeddings:MinSimilarity",0.3),
     direct.Select(p=>p.Id).ToList(),ct);
   var ids=hits.Select(h=>h.ProductId).ToArray();
   var byId=await db.Products.AsNoTracking().Where(p=>ids.Contains(p.Id)).ToDictionaryAsync(p=>p.Id,ct);
   return hits.Where(h=>byId.ContainsKey(h.ProductId)).Select(h=>new RelatedProduct(byId[h.ProductId],h.Similarity)).ToList();
  } catch(Exception ex) when(ex is not OperationCanceledException) {
   // Si pgvector falla, la búsqueda por texto sigue funcionando.
   log.LogError(ex,"Error en la búsqueda semántica");
   return new();
  }
 }
 public IActionResult Error()=>View();
}
