using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TiendaOnline.Data;
using TiendaOnline.Models;
namespace TiendaOnline.Controllers;
[Route("admin/promotions")]
public class PromotionsController(ShopDbContext db,IConfiguration config):Controller {
 private bool Authorized(string? key)=> !string.IsNullOrWhiteSpace(config["Admin:AccessKey"]) && key==config["Admin:AccessKey"];
 [HttpGet]
 public async Task<IActionResult> Index([FromQuery]string? key) {
  if(!Authorized(key)) return Unauthorized();
  ViewBag.Key=key;return View(await db.Promotions.OrderByDescending(p=>p.StartsUtc).ToListAsync());
 }
 [HttpPost("create"),ValidateAntiForgeryToken]
 public async Task<IActionResult> Create([FromForm]string key,[FromForm]string code,[FromForm]string label,[FromForm]decimal percent,[FromForm]DateTime fromUtc,[FromForm]DateTime untilUtc) {
  if(!Authorized(key)) return Unauthorized();
  if(string.IsNullOrWhiteSpace(code)||percent<0||percent>80||untilUtc<=fromUtc) return BadRequest("Promoción inválida");
  db.Promotions.Add(new Promotion{Code=code.ToUpperInvariant().Trim(),Label=label,DiscountPercent=percent,StartsUtc=DateTime.SpecifyKind(fromUtc,DateTimeKind.Utc),EndsUtc=DateTime.SpecifyKind(untilUtc,DateTimeKind.Utc)});
  await db.SaveChangesAsync();return RedirectToAction("Index",new{key});
 }
 [HttpPost("toggle"),ValidateAntiForgeryToken]
 public async Task<IActionResult> Toggle([FromForm]string key,[FromForm]int id) {
  if(!Authorized(key)) return Unauthorized(); var p=await db.Promotions.FindAsync(id);
  if(p==null)return NotFound();p.Enabled=!p.Enabled;await db.SaveChangesAsync();return RedirectToAction("Index",new{key});
 }
}
