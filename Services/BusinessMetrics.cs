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
}
