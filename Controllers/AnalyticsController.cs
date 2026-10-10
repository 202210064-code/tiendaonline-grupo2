using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TiendaOnline.Data;
namespace TiendaOnline.Controllers;
public class AnalyticsController(ShopDbContext db, IConfiguration config):Controller {
 [HttpGet]
 public async Task<IActionResult> Index(string? key, int days=30) {
  // Restrict panel with secret; for production replace with SSO/IAM/identity provider.
  var expected=config["Dashboard:AccessKey"];
  if(string.IsNullOrWhiteSpace(expected) || key != expected) return Unauthorized("Configure Dashboard:AccessKey y proporcione ?key=...; no publique la URL.");
  days=Math.Clamp(days,1,90);var from=DateTime.UtcNow.AddDays(-days);
  var events=await db.BusinessEvents.AsNoTracking().Where(x=>x.CreatedUtc>=from).ToListAsync();
  var paid=await db.Orders.AsNoTracking().Where(x=>x.CreatedUtc>=from && x.Status=="PAGADO").ToListAsync();
  ViewBag.Days=days;ViewBag.Visits=events.Count(x=>x.EventType=="visit");
  ViewBag.Adds=events.Count(x=>x.EventType=="cart_add");
  ViewBag.Checkouts=events.Count(x=>x.EventType=="order_created");
  ViewBag.Paid=paid.Count;ViewBag.Revenue=paid.Sum(x=>x.Total);
  ViewBag.Conversion=(int)ViewBag.Visits>0 ? 100.0*paid.Count/(int)ViewBag.Visits : 0;
  ViewBag.ByDay=paid.GroupBy(x=>x.CreatedUtc.Date).OrderBy(x=>x.Key)
   .Select(x=>new {day=x.Key.ToString("yyyy-MM-dd"),orders=x.Count(),revenue=x.Sum(o=>o.Total)}).ToList();
  return View();
 }
}
