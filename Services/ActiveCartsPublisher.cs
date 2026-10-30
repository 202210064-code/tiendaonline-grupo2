namespace TiendaOnline.Services;

public class ActiveCartsPublisher(IServiceScopeFactory scopes, ActiveCartsTracker tracker,
    IConfiguration config, ILogger<ActiveCartsPublisher> log) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        await Task.Yield(); // no bloquea el arranque de la app mientras conecta a Redis
        if (!config.GetValue("Metrics:CloudWatchEnabled", false) || !tracker.Habilitado) return;
        using var timer = new PeriodicTimer(TimeSpan.FromMinutes(1));
        while (await timer.WaitForNextTickAsync(ct))
        {
            try
            {
                var n = await tracker.CountLastMinutesAsync();
                if (n is null) continue;
                using var scope = scopes.CreateScope();
                await scope.ServiceProvider.GetRequiredService<BusinessMetrics>()
                    .PublishActiveCartsAsync(n.Value, ct);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            { log.LogWarning(ex, "No se pudo publicar ActiveCarts"); }
        }
    }
}