using Microsoft.AspNetCore.Mvc;
using TiendaOnline.Data;
using TiendaOnline.Services;
namespace TiendaOnline.Controllers;
public class CartController(CartService cart,ShopDbContext db,BusinessEvents events,BusinessMetrics metrics):Controller {
 public async Task<IActionResult> Index()=>View(await cart.ViewAsync(HttpContext.Session));
 [HttpPost,ValidateAntiForgeryToken]
 public async Task<IActionResult> Add(int id,int quantity=1) {
  if(await db.Products.FindAsync(id) is { Stock: > 0 } p) {
   var agregadas=Math.Min(quantity,p.Stock);
   cart.Add(HttpContext.Session,id,agregadas);
   await events.RecordAsync("cart_add",productId:id);
   await metrics.CartItemsAddedAsync(agregadas);
   // Marca de sesión: cuenta el carrito una sola vez.
   if(HttpContext.Session.GetString("cart_activo") is null) {
    HttpContext.Session.SetString("cart_activo","1");
    await metrics.CartCreatedAsync();
   }
   TempData["Message"]="Producto agregado al carrito";
  }
  return RedirectToAction("Index","Home");
 }
 [HttpPost,ValidateAntiForgeryToken]
 public IActionResult Remove(int id) { cart.Remove(HttpContext.Session,id); return RedirectToAction("Index"); }
}