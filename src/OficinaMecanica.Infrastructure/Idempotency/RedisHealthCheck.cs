using Microsoft.Extensions.Diagnostics.HealthChecks;
using StackExchange.Redis;

namespace OficinaMecanica.Infrastructure.Idempotency;

public class RedisHealthCheck : IHealthCheck
{
    private readonly IConnectionMultiplexer _connectionMultiplexer;

    public RedisHealthCheck(IConnectionMultiplexer connectionMultiplexer) =>
        _connectionMultiplexer = connectionMultiplexer;

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            await _connectionMultiplexer.GetDatabase().PingAsync();
            return HealthCheckResult.Healthy();
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("Falha ao conectar ao Redis.", ex);
        }
    }
}
