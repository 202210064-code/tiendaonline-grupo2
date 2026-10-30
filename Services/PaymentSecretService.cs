using Amazon.SecretsManager;
using Amazon.SecretsManager.Model;
using System.Text.Json;

namespace TiendaOnline.Services;

public class PaymentSecretService(IAmazonSecretsManager secretsManager, IConfiguration config)
{
    public async Task<string> GetAccessTokenAsync(CancellationToken ct = default)
    {
        // Si hay una clave fija en appsettings (para desarrollo local), la usa
        var localToken = config["MercadoPago:TestAccessToken"];
        if (!string.IsNullOrEmpty(localToken))
        {
            return localToken;
        }

        // De lo contrario, lee el secreto desde AWS Secrets Manager (ARN inyectado en /etc/urponline.env)
        var secretArn = config["MercadoPago:SecretArn"];
        if (string.IsNullOrEmpty(secretArn))
        {
            return string.Empty;
        }

        var request = new GetSecretValueRequest { SecretId = secretArn };
        var response = await secretsManager.GetSecretValueAsync(request, ct);

        using var doc = JsonDocument.Parse(response.SecretString);
        if (doc.RootElement.TryGetProperty("AccessToken", out var tokenElement))
        {
            return tokenElement.GetString() ?? string.Empty;
        }

        return response.SecretString;
    }
}