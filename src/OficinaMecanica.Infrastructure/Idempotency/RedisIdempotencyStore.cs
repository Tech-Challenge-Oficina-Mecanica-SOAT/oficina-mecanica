using OficinaMecanica.Application.Interfaces;
using StackExchange.Redis;

namespace OficinaMecanica.Infrastructure.Idempotency;

public class RedisIdempotencyStore : IIdempotencyStore
{
    private readonly IConnectionMultiplexer _connectionMultiplexer;

    public RedisIdempotencyStore(IConnectionMultiplexer connectionMultiplexer) =>
        _connectionMultiplexer = connectionMultiplexer;

    private IDatabase Database => _connectionMultiplexer.GetDatabase();

    public async Task<string?> ObterAsync(string chave)
    {
        var valor = await Database.StringGetAsync(chave);
        return valor.HasValue ? valor.ToString() : null;
    }

    public async Task SalvarAsync(string chave, string valor, TimeSpan expiracao) =>
        await Database.StringSetAsync(chave, valor, expiracao);
}
