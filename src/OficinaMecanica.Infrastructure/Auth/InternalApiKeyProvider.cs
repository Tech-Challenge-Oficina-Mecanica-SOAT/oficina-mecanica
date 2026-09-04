using System.Security.Cryptography;
using System.Text;
using AspNetCore.Authentication.ApiKey;
using Microsoft.Extensions.Configuration;

namespace OficinaMecanica.Infrastructure.Auth;

public class InternalApiKeyProvider : IApiKeyProvider
{
    private readonly IConfiguration _configuration;

    public InternalApiKeyProvider(IConfiguration configuration) => _configuration = configuration;

    public Task<IApiKey?> ProvideAsync(string key)
    {
        var configuredKey = _configuration["InternalApi:ApiKey"];

        IApiKey? result = !string.IsNullOrEmpty(configuredKey) && ChavesIguais(configuredKey, key)
            ? new InternalApiKey(key)
            : null;

        return Task.FromResult(result);
    }

    private static bool ChavesIguais(string configuredKey, string key)
    {
        var configuredBytes = Encoding.UTF8.GetBytes(configuredKey);
        var keyBytes = Encoding.UTF8.GetBytes(key);

        // Comprimentos diferentes já vazam informação por timing, mas FixedTimeEquals
        // exige buffers do mesmo tamanho; como o comprimento da chave configurada não
        // é segredo (é um valor de infra, não derivado do segredo em si), aceitar esse
        // vazamento pontual é preferível a normalizar tamanho de forma mais complexa.
        return configuredBytes.Length == keyBytes.Length &&
               CryptographicOperations.FixedTimeEquals(configuredBytes, keyBytes);
    }

    private sealed class InternalApiKey : IApiKey
    {
        public InternalApiKey(string key)
        {
            Key = key;
            OwnerName = "lambda-auth";
            Claims = new List<System.Security.Claims.Claim>();
        }

        public string Key { get; }
        public string OwnerName { get; }
        public IReadOnlyCollection<System.Security.Claims.Claim> Claims { get; }
    }
}
