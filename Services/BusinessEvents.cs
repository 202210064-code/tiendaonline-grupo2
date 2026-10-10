using TiendaOnline.Data;
using TiendaOnline.Models;
namespace TiendaOnline.Services;
public class BusinessEvents(ShopDbContext db, IHttpContextAccessor context) {
 public async Task RecordAsync(string type,int? productId=null,int? orderId=null,decimal? amount=null) {
  var ctx=context.HttpContext;
  string key=ctx?.Session.Id ?? "system";
  db.BusinessEvents.Add(new BusinessEvent {EventType=type,SessionKey=key,
   ProductId=productId,OrderId=orderId,Amount=amount,
   Campaign=ctx?.Request.Query["campaign"].ToString(), Country=ctx?.Request.Headers["CloudFront-Viewer-Country"].ToString()});
  await db.SaveChangesAsync();
 }
}
