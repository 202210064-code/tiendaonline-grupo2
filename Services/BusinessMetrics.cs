using Amazon.CloudWatch;
using Amazon.CloudWatch.Model;
namespace TiendaOnline.Services;

// Publica métricas de negocio en Amazon CloudWatch con el AWS SDK (namespace "URPOnline").
// Se ven en el dashboard URPOnline-<pila>. En la EC2 las credenciales vienen del rol IAM (sin claves).
// Se activa con Metrics:CloudWatchEnabled=true (la plantilla lo pone en las EC2; en tu PC queda apagado).
public class BusinessMetrics(IAmazonCloudWatch cloudWatch, IConfiguration config, ILogger<BusinessMetrics> log) {
 public async Task OrderCreatedAsync(decimal amount, CancellationToken ct = default) {
  if (!config.GetValue("Metrics:CloudWatchEnabled", false)) return;
  try {
   await cloudWatch.PutMetricDataAsync(new PutMetricDataRequest {
    Namespace = "URPOnline",
    MetricData = new List<MetricDatum> {
     new() { MetricName = "OrdersCreated", Value = 1, Unit = StandardUnit.Count },
     new() { MetricName = "OrderAmount", Value = (double)amount, Unit = StandardUnit.None }
    }
   }, ct);
  } catch (Exception ex) when (ex is not OperationCanceledException) {
   // La venta ya se guardó en PostgreSQL; si CloudWatch falla, solo se registra el aviso.
   log.LogWarning(ex, "No se pudo publicar la métrica del pedido en CloudWatch");
  }
 }

 // Carritos con al menos un producto que aún no terminan en pedido (por instancia).
 static int activeCarts;

 // Primer producto agregado a un carrito nuevo.
 public Task CartCreatedAsync(CancellationToken ct = default) =>
  PublishAsync("ActiveCarts", Interlocked.Increment(ref activeCarts), StandardUnit.Count, ct);

 // Carrito que termina en pedido o se vacía.
 public Task CartClosedAsync(CancellationToken ct = default) {
  int actual, siguiente;
  do { actual = activeCarts; siguiente = Math.Max(0, actual - 1); }
  while (Interlocked.CompareExchange(ref activeCarts, siguiente, actual) != actual);
  return PublishAsync("ActiveCarts", siguiente, StandardUnit.Count, ct);
 }

 // Unidades agregadas al carrito en una acción.
 public Task CartItemsAddedAsync(int quantity, CancellationToken ct = default) =>
  PublishAsync("CartItemsAdded", quantity, StandardUnit.Count, ct);

 async Task PublishAsync(string name, double value, StandardUnit unit, CancellationToken ct) {
  if (!config.GetValue("Metrics:CloudWatchEnabled", false)) return;
  try {
   await cloudWatch.PutMetricDataAsync(new PutMetricDataRequest {
    Namespace = "URPOnline",
    MetricData = new List<MetricDatum> { new() { MetricName = name, Value = value, Unit = unit } }
   }, ct);
  } catch (Exception ex) when (ex is not OperationCanceledException) {
   // La operación del carrito ya se hizo; si CloudWatch falla, solo se registra el aviso.
   log.LogWarning(ex, "No se pudo publicar la métrica {Metrica} en CloudWatch", name);
  }
 }
}