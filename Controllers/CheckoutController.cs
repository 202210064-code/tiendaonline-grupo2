using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Data;
using TiendaOnline.Data;
using TiendaOnline.Models;
using TiendaOnline.Services;
namespace TiendaOnline.Controllers;
public class CheckoutController(ShopDbContext db,CartService cart,BusinessEvents events):Controller {
 [HttpGet] public async Task<IActionResult> Index() {
  var vm=await cart.ViewAsync(HttpContext.Session);
  if(vm.Lines.Count==0) return RedirectToAction("Index","Cart");
  ViewBag.Cart=vm; return View(new CheckoutInput());
 }
 [HttpPost,ValidateAntiForgeryToken]
 public async Task<IActionResult> Index(CheckoutInput input) {
  var vm=await cart.ViewAsync(HttpContext.Session);
  if(vm.Lines.Count==0) return RedirectToAction("Index","Cart");
  if(!ModelState.IsValid) { ViewBag.Cart=vm; return View(input); }
  await using var tx=await db.Database.BeginTransactionAsync(IsolationLevel.ReadCommitted);
  try {
   // La actualización condicional impide vender más unidades que el stock disponible.
   foreach(var line in vm.Lines) {
    int count=await db.Products.Where(p=>p.Id==line.Product.Id && p.Stock>=line.Quantity)
      .ExecuteUpdateAsync(updates=>updates.SetProperty(p=>p.Stock,p=>p.Stock-line.Quantity));
    if(count!=1) {
     await tx.RollbackAsync();
     ModelState.AddModelError("","No hay stock suficiente de: "+line.Product.Name);
     ViewBag.Cart=await cart.ViewAsync(HttpContext.Session); return View(input);
    }
   }
   // Se vuelven a leer los precios de la base de datos, no desde el navegador.
   var ids=vm.Lines.Select(x=>x.Product.Id).ToArray();
   var products=await db.Products.AsNoTracking().Where(p=>ids.Contains(p.Id)).ToDictionaryAsync(p=>p.Id);
   var order=new Order { CustomerName=input.CustomerName,Email=input.Email,Address=input.Address,
    Items=vm.Lines.Select(l=>new OrderItem{ ProductId=l.Product.Id,ProductName=products[l.Product.Id].Name,
     Quantity=l.Quantity,UnitPrice=products[l.Product.Id].Price }).ToList() };
   order.Total=order.Items.Sum(x=>x.Quantity*x.UnitPrice);
   // Apply the largest active promotion consistently to new orders.
   var now=DateTime.UtcNow;
   var promo=await db.Promotions.AsNoTracking().Where(x=>x.Enabled && x.StartsUtc<=now && x.EndsUtc>now)
      .OrderByDescending(x=>x.DiscountPercent).FirstOrDefaultAsync();
   if(promo!=null) order.Total=Math.Round(order.Total*(100m-promo.DiscountPercent)/100m,2);
   // Apply the largest active promotion consistently to new orders.
   var now=DateTime.UtcNow;
   var promo=await db.Promotions.AsNoTracking().Where(x=>x.Enabled && x.StartsUtc<=now && x.EndsUtc>now)
      .OrderByDescending(x=>x.DiscountPercent).FirstOrDefaultAsync();
   if(promo!=null) order.Total=Math.Round(order.Total*(100m-promo.DiscountPercent)/100m,2);
   db.Orders.Add(order); await db.SaveChangesAsync(); await tx.CommitAsync();
   cart.Clear(HttpContext.Session);
   await events.RecordAsync("order_created",orderId:order.Id,amount:order.Total);
   await events.RecordAsync("order_created",orderId:order.Id,amount:order.Total);
   return RedirectToAction("Success",new{id=order.Id});
  } catch {
   await tx.RollbackAsync(); throw;
  }
 }
 [HttpGet] public IActionResult Success(int id) { ViewBag.OrderId=id; return View(); }
}
