using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TiendaOnline.Data;
using TiendaOnline.Services;
namespace TiendaOnline.Controllers;
public class HomeController(ShopDbContext db,BusinessEvents events) : Controller {
 public async Task<IActionResult> Index(string? category,string? q) {
  var products=db.Products.AsNoTracking().AsQueryable();
  if(!string.IsNullOrWhiteSpace(category)) products=products.Where(x=>x.Category==category);
  if(!string.IsNullOrWhiteSpace(q)) products=products.Where(x=>x.Name.ToLower().Contains(q.ToLower()));
  ViewBag.Query=q; ViewBag.Category=category;
  await events.RecordAsync("visit");
  return View(await products.OrderBy(x=>x.Id).ToListAsync());
 }
 public IActionResult Error()=>View();
}
