using System.Security.Cryptography;
using System.Text;
using StackExchange.Redis;

namespace TiendaOnline.Services;

public class ActiveCartsTracker(IConfiguration config, ILogger<ActiveCartsTracker> log)
{
    const string Key = "urponline:activecarts";
    readonly Lazy<ConnectionMultiplexer?> redis = new(() => Conectar(config, log));

    public bool Habilitado => redis.Value is not null;

    static ConnectionMultiplexer? Conectar(IConfiguration config, ILogger log)
    {
        var cs = config.GetConnectionString("Redis");
        if (string.IsNullOrWhiteSpace(cs)) return null;
        try
        {
            var opciones = ConfigurationOptions.Parse(cs);
            opciones.AbortOnConnectFail = false;
            return ConnectionMultiplexer.Connect(opciones);
        }
        catch (Exception ex) { log.LogWarning(ex, "ActiveCarts: no se pudo conectar a Redis"); return null; }
    }

    // El identificador de sesión es una credencial: nunca se guarda tal cual, solo su hash.
    public static string HashSession(string sessionId) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(sessionId)))[..16];

    public async Task TouchAsync(string sessionId)
    {
        if (redis.Value is null) return;
        try
        {
            await redis.Value.GetDatabase().SortedSetAddAsync(Key, HashSession(sessionId),
                DateTimeOffset.UtcNow.ToUnixTimeSeconds());
        }
        catch (Exception ex) { log.LogWarning(ex, "ActiveCarts: no se pudo anotar la sesión"); }
    }

    public async Task<long?> CountLastMinutesAsync(int minutes = 5)
    {
        if (redis.Value is null) return null;
        var db = redis.Value.GetDatabase();
        var corte = DateTimeOffset.UtcNow.ToUnixTimeSeconds() - minutes * 60;
        await db.SortedSetRemoveRangeByScoreAsync(Key, double.NegativeInfinity, corte);
        return await db.SortedSetLengthAsync(Key, corte, double.PositiveInfinity);
    }
}