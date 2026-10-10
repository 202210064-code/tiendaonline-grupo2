using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TiendaOnline.Data;
using TiendaOnline.Models;
using TiendaOnline.Services;
namespace TiendaOnline.Controllers;
public class HomeController(ShopDbContext db,BusinessEvents events,EmbeddingService embeddings,ProductVectorStore vectors,
  IConfiguration config,ILogger<HomeController> log) : Controller {
 public async Task<IActionResult> Index(string? category,string? q,string? sort,decimal? min,decimal? max,bool stock,CancellationToken ct) {
  var vm=new StorePageViewModel{Query=q?.Trim(),Category=category,Sort=sort ?? "relevancia",Min=min,Max=max,InStockOnly=stock};
  var products=db.Products.AsNoTracking().AsQueryable();
  if(!string.IsNullOrWhiteSpace(vm.Query)) { var term=vm.Query.ToLower(); products=products.Where(x=>x.Name.ToLower().Contains(term) || x.Description.ToLower().Contains(term)); }
  // Conteos por categoría antes de aplicar el filtro de categoría (barra lateral).
  vm.CategoryCounts=(await products.GroupBy(x=>x.Category).Select(g=>new{g.Key,Count=g.Count()}).OrderBy(x=>x.Key).ToListAsync(ct))
    .Select(x=>(x.Key,x.Count)).ToList();
  if(!string.IsNullOrWhiteSpace(category)) products=products.Where(x=>x.Category==category);
  if(min.HasValue) products=products.Where(x=>x.Price>=min.Value);
  if(max.HasValue) products=products.Where(x=>x.Price<max.Value);
  if(stock) products=products.Where(x=>x.Stock>0);
  products=vm.Sort switch {
   "precio-asc"=>products.OrderBy(x=>x.Price),
   "precio-desc"=>products.OrderByDescending(x=>x.Price),
   "nombre"=>products.OrderBy(x=>x.Name),
   _=>products.OrderBy(x=>x.Id)
  };
  vm.Results=await products.ToListAsync(ct);
  if(!vm.IsSearch) vm.CategoryTiles=vm.Results.GroupBy(x=>x.Category).OrderBy(g=>g.Key).Select(g=>(g.Key,g.Take(4).ToList())).ToList();
  if(!string.IsNullOrWhiteSpace(vm.Query)) {
   if(embeddings.IsAvailable) vm.Related=await RelatedAsync(vm.Query,vm.Results,ct);
   else vm.SemanticUnavailable=embeddings.UnavailableReason;
  }
  await events.RecordAsync("visit");
  return View(vm);
 }

 // Productos semánticamente parecidos a la búsqueda (pgvector), excluyendo los que ya coincidieron por texto.
 private async Task<List<RelatedProduct>> RelatedAsync(string q,List<Product> direct,CancellationToken ct,double? minSimilarity=null) {
  try {
   if(!await vectors.EnsureSchemaAsync(ct)) return new();
   var hits=await vectors.SearchAsync(embeddings.Embed(q),
     config.GetValue("Embeddings:RelatedLimit",4),minSimilarity ?? config.GetValue("Embeddings:MinSimilarity",0.3),
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
 // Sugerencias mientras se escribe: coincidencias por texto + relacionados por pgvector.
 [HttpGet] public async Task<IActionResult> Suggest(string? q,CancellationToken ct) {
  q=q?.Trim();
  if(string.IsNullOrEmpty(q) || q.Length<2) return Json(Array.Empty<object>());
  var term=q.ToLower();
  var direct=await db.Products.AsNoTracking()
    .Where(x=>x.Name.ToLower().Contains(term) || x.Category.ToLower().Contains(term))
    .OrderBy(x=>x.Name).Take(6).ToListAsync(ct);
  var related=embeddings.IsAvailable ? await RelatedAsync(q,direct,ct,config.GetValue("Embeddings:SuggestMinSimilarity",0.45)) : new List<RelatedProduct>();
  // Términos de búsqueda (como Amazon): primero los que coinciden por texto, luego los relacionados por significado.
  var terms=direct.Select(p=>p.Name).Concat(related.Select(r=>r.Product.Name))
    .Distinct(StringComparer.OrdinalIgnoreCase).Take(8).Select(n=>n.ToLowerInvariant());
  return Json(terms);
 }

 public IActionResult Error()=>View();
}
