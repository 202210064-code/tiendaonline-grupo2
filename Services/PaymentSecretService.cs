using Amazon.SecretsManager;
using Amazon.SecretsManager.Model;
using System.Text.Json;

namespace TiendaOnline.Services;

public class PaymentSecretService(IAmazonSecretsManager secretsManager, IConfiguration config)
{
    public async Task<string> GetAccessTokenAsync(CancellationToken ct = default)
    {
        // 1. Si hay una clave fija en appsettings (para desarrollo local), la usa
        var localToken = config["MercadoPago:TestAccessToken"];
        if (!string.IsNullOrWhiteSpace(localToken))
        {
            return localToken.Trim();
        }

        // 2. Lee el ARN/Nombre del secreto desde AWS Secrets Manager
        var secretArn = config["MercadoPago:SecretArn"];
        if (string.IsNullOrWhiteSpace(secretArn))
        {
            return string.Empty;
        }

        try
        {
            var request = new GetSecretValueRequest { SecretId = secretArn };
            var response = await secretsManager.GetSecretValueAsync(request, ct);

            if (string.IsNullOrWhiteSpace(response.SecretString))
            {
                return string.Empty;
            }

            var rawSecret = response.SecretString.Trim();

            // 3. Si el secreto está guardado como formato JSON válido
            if (rawSecret.StartsWith("{"))
            {
                using var doc = JsonDocument.Parse(rawSecret);
                if (doc.RootElement.TryGetProperty("AccessToken", out var tokenElement))
                {
                    return tokenElement.GetString()?.Trim() ?? string.Empty;
                }
                if (doc.RootElement.TryGetProperty("access_token", out var altTokenElement))
                {
                    return altTokenElement.GetString()?.Trim() ?? string.Empty;
                }
            }

            // 4. Si se guardó como texto plano (ej. APP_USR-xxx... o TEST-xxx...)
            return rawSecret;
        }
        catch
        {
            return string.Empty;
        }
    }
}