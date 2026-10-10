using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TiendaOnline.Data;
using TiendaOnline.Services;
namespace TiendaOnline.Controllers;
[Route("payments")]
public class PaymentsController(ShopDbContext db, BusinessEvents events, IConfiguration config):Controller {
 [HttpPost("simulate"),ValidateAntiForgeryToken]
 public async Task<IActionResult> Simulate([FromForm]int orderId,[FromForm]string? demoKey) {
  if(!config.GetValue<bool>("Payments:EnableSimulation") || demoKey != config["Payments:DemoKey"])
   return Forbid();
  var order=await db.Orders.FirstOrDefaultAsync(o=>o.Id==orderId);
  if(order is null)return NotFound();
  if(order.Status=="PENDIENTE_PAGO"){
   order.Status="PAGADO";await db.SaveChangesAsync();
   await events.RecordAsync("payment_confirmed",orderId:order.Id,amount:order.Total);
  }
  return Ok(new{order.Id,order.Status,Demo=true});
 }
}
