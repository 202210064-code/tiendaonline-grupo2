using System.Text.Json;
using TiendaOnline.Data;
using TiendaOnline.Models;
using Microsoft.EntityFrameworkCore;
namespace TiendaOnline.Services;
public class CartService(ShopDbContext db) {
 private const string Key = "cart-v1";
 private readonly ShopDbContext _db = db;
 public List<CartItem> Read(ISession s) {
  var json=s.GetString(Key);
  try { return string.IsNullOrWhiteSpace(json) ? new() : JsonSerializer.Deserialize<List<CartItem>>(json) ?? new(); }
  catch(JsonException) { return new(); }
 }
 public void Write(ISession s,List<CartItem> items) => s.SetString(Key,JsonSerializer.Serialize(items));
 public void Add(ISession s,int id,int qty) {
  qty=Math.Clamp(qty,1,99);
  var items=Read(s); var old=items.FirstOrDefault(x=>x.ProductId==id);
  if(old==null) items.Add(new CartItem{ProductId=id,Quantity=qty});
  else old.Quantity=Math.Min(99,old.Quantity+qty);
  Write(s,items);
 }
 public async Task<CartViewModel> ViewAsync(ISession s) {
  var items=Read(s); var ids=items.Select(x=>x.ProductId).ToArray();
  var products=await _db.Products.AsNoTracking().Where(p=>ids.Contains(p.Id)).ToListAsync();
  return new CartViewModel{Lines=items.Join(products,i=>i.ProductId,p=>p.Id,(i,p)=>new CartLine{Product=p,Quantity=i.Quantity}).ToList()};
 }
 public void Remove(ISession s,int id) { var items=Read(s); items.RemoveAll(x=>x.ProductId==id); Write(s,items); }
 public void Clear(ISession s)=>s.Remove(Key);
}
