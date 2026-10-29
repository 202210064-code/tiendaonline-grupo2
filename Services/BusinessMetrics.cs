using Amazon.CloudWatch;
using Amazon.CloudWatch.Model;
namespace TiendaOnline.Services;


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

 // Unidades agregadas al carrito en una acción.
 public Task CartItemsAddedAsync(int quantity, CancellationToken ct = default) =>
  PublishAsync("CartItemsAdded", quantity, StandardUnit.Count, ct);

 // Personas distintas con carrito activo en los últimos 5 minutos (lo calcula ActiveCartsPublisher).
 public Task PublishActiveCartsAsync(double valor, CancellationToken ct = default) =>
  PublishAsync("ActiveCarts", valor, StandardUnit.Count, ct);

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