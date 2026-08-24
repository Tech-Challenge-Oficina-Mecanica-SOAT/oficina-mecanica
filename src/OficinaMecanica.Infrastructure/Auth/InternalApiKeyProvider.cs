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

        IApiKey? result = !string.IsNullOrEmpty(configuredKey) && configuredKey == key
            ? new InternalApiKey(key)
            : null;

        return Task.FromResult(result);
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
