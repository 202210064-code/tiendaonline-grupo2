using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Data;
using TiendaOnline.Data;
using TiendaOnline.Models;
using TiendaOnline.Services;
// --- LIBRERÍAS DE MERCADO PAGO ---
using MercadoPago.Config;
using MercadoPago.Client.Preference;
using MercadoPago.Client.Payment;          // <-- NUEVO (3e): para consultar el pago
using MercadoPago.Resource.Preference;
 
namespace TiendaOnline.Controllers;
 
public class CheckoutController(
    ShopDbContext db,
    CartService cart,
    BusinessEvents events,
    BusinessMetrics metrics,
    PaymentSecretService paymentSecretService) : Controller {
 
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
 
  Order order;
 
  // =========================================================================
  // 1. TRANSACCIÓN BASE DE DATOS (Corta y segura para PgBouncer)
  // =========================================================================
  await using (var tx = await db.Database.BeginTransactionAsync(IsolationLevel.ReadCommitted)) {
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
    order=new Order {
     CustomerName=input.CustomerName,
     Email=input.Email,
     Address=input.Address,
     Status="PENDIENTE_PAGO",
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
 
    // Confirmamos la transacción en PostgreSQL para liberar la conexión en PgBouncer
    await tx.CommitAsync();
 
   } catch {
    await tx.RollbackAsync();
    throw;
   }
  }
 
  // Limpieza del carrito y registro de eventos/métricas
  cart.Clear(HttpContext.Session);
  await events.RecordAsync("order_created",orderId:order.Id,amount:order.Total);
  await metrics.OrderCreatedAsync(order.Total,HttpContext.RequestAborted);
 
  // =========================================================================
  // 2. CREACIÓN DE PREFERENCIA DE PAGO EN MERCADO PAGO (Fuera de la transacción)
  // =========================================================================
 
  // A. Obtención del token de prueba desde Secrets Manager (Indicador 3c)
  string accessToken = await paymentSecretService.GetAccessTokenAsync(HttpContext.RequestAborted);
  MercadoPagoConfig.AccessToken = accessToken;
 
  // B. Construcción del request de la preferencia de pago
  var preferenceRequest = new PreferenceRequest
  {
      Items = order.Items.Select(i => new PreferenceItemRequest
      {
          Title = i.ProductName,
          Quantity = i.Quantity,
          UnitPrice = i.UnitPrice,
          CurrencyId = "PEN"
      }).ToList(),
 
      // Vincula la orden de PostgreSQL con Mercado Pago
      ExternalReference = order.Id.ToString(),
 
      Payer = new PreferencePayerRequest
      {
          Name = input.CustomerName,
          Email = input.Email
      },
 
      // FORZAMOS EL PROTOCOLO HTTPS EN LAS BACKURLS
      BackUrls = new PreferenceBackUrlsRequest
      {
          Success = $"https://{Request.Host}/Checkout/Success?id={order.Id}",
          Failure = $"https://{Request.Host}/Checkout/Success?id={order.Id}",
          Pending = $"https://{Request.Host}/Checkout/Success?id={order.Id}"
      },
      AutoReturn = "approved"
  };
 
  // C. Crear la preferencia invocando al SDK de Mercado Pago
  var client = new PreferenceClient();
  Preference preference = await client.CreateAsync(preferenceRequest, cancellationToken: HttpContext.RequestAborted);
 
  // D. Redirigir al cliente al Checkout de Sandbox de Mercado Pago
  string checkoutUrl = string.IsNullOrEmpty(preference.SandboxInitPoint)
      ? preference.InitPoint
      : preference.SandboxInitPoint;
 
  return Redirect(checkoutUrl);
 }
 
 // =========================================================================
 // 3e. CONFIRMACIÓN SEGURA AL VOLVER DE MERCADO PAGO
 //  - Consulta el pago desde el SERVIDOR (no se confía en ?status= de la URL)
 //  - Valida que el pago corresponde a este pedido y al monto
 //  - Solo marca PAGADO si está "approved"
 //  - Es idempotente: recargar la página no registra nada dos veces
 // =========================================================================
 [HttpGet]
 public async Task<IActionResult> Success(int id, long? payment_id, long? collection_id) {
  var order = await db.Orders.AsNoTracking().FirstOrDefaultAsync(o => o.Id == id);
  if (order == null) return NotFound();
 
  string result;
  long? pid = payment_id ?? collection_id;
 
  if (order.Status == "PAGADO") {
   // Recarga: ya estaba registrado, no se hace nada más
   result = "YA_REGISTRADO";
  }
  else if (pid == null) {
   // Volvió sin datos de pago (canceló o cerró antes de pagar)
   result = "PENDIENTE";
  }
  else {
   try {
    // Consulta del pago DESDE EL SERVIDOR con el token guardado en Secrets Manager
    MercadoPagoConfig.AccessToken = await paymentSecretService.GetAccessTokenAsync(HttpContext.RequestAborted);
    var payment = await new PaymentClient().GetAsync(pid.Value, cancellationToken: HttpContext.RequestAborted);
 
    // El pago debe pertenecer a ESTE pedido y por el monto correcto
    bool coincide = payment.ExternalReference == order.Id.ToString()
                 && Math.Abs((payment.TransactionAmount ?? 0m) - order.Total) < 0.01m;
 
    if (!coincide) {
     result = "INVALIDO";
    }
    else if (payment.Status == "approved") {
     // Actualización condicional: solo UNA petición puede pasar de PENDIENTE_PAGO a PAGADO
       int filas = await db.Orders
    .Where(o => o.Id == id && o.Status == "PENDIENTE_PAGO")
    .ExecuteUpdateAsync(s => s.SetProperty(o => o.Status, "PAGADO"));
 
     if (filas == 1) {
      await events.RecordAsync("order_paid", orderId: order.Id, amount: order.Total);
      result = "PAGADO";
     } else {
      result = "YA_REGISTRADO"; // otra petición lo marcó justo antes
     }
    }
    else if (payment.Status == "rejected" || payment.Status == "cancelled") {
     result = "RECHAZADO";
    }
    else {
     result = "PENDIENTE"; // in_process, pending, etc.
    }
   }
   catch (Exception) {
    // Si no se puede verificar, NUNCA se marca PAGADO
    result = "ERROR_VERIFICACION";
   }
  }
 
  // Se vuelve a leer el pedido para mostrar su estado real
  ViewBag.Order = await db.Orders.AsNoTracking().FirstAsync(o => o.Id == id);
  ViewBag.Result = result;
  ViewBag.PaymentId = pid;
  return View();
 }
}