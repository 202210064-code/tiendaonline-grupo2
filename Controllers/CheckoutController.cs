using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Data;
using TiendaOnline.Data;
using TiendaOnline.Models;
using TiendaOnline.Services;
// --- NUEVAS LIBRERÍAS DE MERCADO PAGO ---
using MercadoPago.Config;
using MercadoPago.Client.Preference;
using MercadoPago.Resource.Preference;

namespace TiendaOnline.Controllers;

public class CheckoutController(
    ShopDbContext db,
    CartService cart,
    BusinessEvents events,
    BusinessMetrics metrics,
    PaymentSecretService paymentSecretService) : Controller { // <-- MODIFICADO: Inyección de PaymentSecretService

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
   
   // --- EL PEDIDO SE CREA EN ESTADO PENDIENTE_PAGO EN LA BD ---
   var order=new Order { 
    CustomerName=input.CustomerName,
    Email=input.Email,
    Address=input.Address,
    Status="PENDIENTE_PAGO", // <-- MODIFICADO: Estado inicial obligatorio
    Items=vm.Lines.Select(l=>new OrderItem{ 
     ProductId=l.Product.Id,
     ProductName=products[l.Product.Id].Name,
     Quantity=l.Quantity,
     UnitPrice=products[l.Product.Id].Price 
    }).ToList() 
   };
   order.Total=order.Items.Sum(x=>x.Quantity*x.UnitPrice);
   
   // Aplica promociones activas
   var now=DateTime.UtcNow;
   var promo=await db.Promotions.AsNoTracking().Where(x=>x.Enabled && x.StartsUtc<=now && x.EndsUtc>now)
      .OrderByDescending(x=>x.DiscountPercent).FirstOrDefaultAsync();
   if(promo!=null) order.Total=Math.Round(order.Total*(100m-promo.DiscountPercent)/100m,2);
   
   db.Orders.Add(order); 
   await db.SaveChangesAsync(); 
   await tx.CommitAsync();

   cart.Clear(HttpContext.Session);
   await events.RecordAsync("order_created",orderId:order.Id,amount:order.Total);
   await metrics.OrderCreatedAsync(order.Total,HttpContext.RequestAborted);

   // =========================================================================
   // --- CÓDIGO AGREGADO: CREACIÓN DE PREFERENCIA DE PAGO EN MERCADO PAGO ---
   // =========================================================================

   // 1. Obtención del token de prueba desde Secrets Manager (Indicador 3c)
   string accessToken = await paymentSecretService.GetAccessTokenAsync(HttpContext.RequestAborted);
   MercadoPagoConfig.AccessToken = accessToken;

   // 2. Construcción del request de la preferencia de pago
   var preferenceRequest = new PreferenceRequest
   {
       Items = order.Items.Select(i => new PreferenceItemRequest
       {
           Title = i.ProductName,
           Quantity = i.Quantity,
           UnitPrice = i.UnitPrice,
           CurrencyId = "PEN"
       }).ToList(),
       
       // Importante: Vincula la orden de PostgreSQL con Mercado Pago
       ExternalReference = order.Id.ToString(), 
       
       Payer = new PreferencePayerRequest
       {
           Name = input.CustomerName,
           Email = input.Email
       },
       
       // URLs a las que regresa el usuario tras el intento de pago en Sandbox
       BackUrls = new PreferenceBackUrlsRequest
       {
           Success = $"{Request.Scheme}://{Request.Host}/Checkout/Success",
           Failure = $"{Request.Scheme}://{Request.Host}/Checkout/Success",
           Pending = $"{Request.Scheme}://{Request.Host}/Checkout/Success"
       },
       AutoReturn = "approved"
   };

   // 3. Crear la preferencia invocando al SDK de Mercado Pago
   var client = new PreferenceClient();
   Preference preference = await client.CreateAsync(preferenceRequest, cancellationToken: HttpContext.RequestAborted);

   // 4. Redirigir al cliente al Checkout de Sandbox de Mercado Pago
   // (Usa SandboxInitPoint para pruebas locales o InitPoint para producción/HTTPS)
   string checkoutUrl = string.IsNullOrEmpty(preference.SandboxInitPoint) 
       ? preference.InitPoint 
       : preference.SandboxInitPoint;

   return Redirect(checkoutUrl);

  } catch {
   await tx.RollbackAsync(); throw;
  }
 }

 [HttpGet] public IActionResult Success(int id) { ViewBag.OrderId=id; return View(); }
}