using Microsoft.AspNetCore.Mvc;
using TiendaOnline.Data;
using TiendaOnline.Services;
namespace TiendaOnline.Controllers;
public class CartController(CartService cart,ShopDbContext db,BusinessEvents events,BusinessMetrics metrics,ActiveCartsTracker tracker):Controller {
 public async Task<IActionResult> Index()=>View(await cart.ViewAsync(HttpContext.Session));
 [HttpPost,ValidateAntiForgeryToken]
 public async Task<IActionResult> Add(int id,int quantity=1) {
  if(await db.Products.FindAsync(id) is { Stock: > 0 } p) {
   var agregadas=Math.Min(quantity,p.Stock);
   cart.Add(HttpContext.Session,id,agregadas);
   await events.RecordAsync("cart_add",productId:id);
   await metrics.CartItemsAddedAsync(agregadas);
   // Anota en Redis la sesión (con hash) y la hora; en cada Add, no solo la primera vez.
   await tracker.TouchAsync(HttpContext.Session.Id);
   TempData["Message"]="Producto agregado al carrito";
  }
  return RedirectToAction("Index","Home");
 }
 [HttpPost,ValidateAntiForgeryToken]
 public IActionResult Remove(int id) { cart.Remove(HttpContext.Session,id); return RedirectToAction("Index"); }
}