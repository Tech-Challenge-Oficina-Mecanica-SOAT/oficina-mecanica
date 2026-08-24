namespace OficinaMecanica.Application.Interfaces;

public interface IIdempotencyStore
{
    Task<string?> ObterAsync(string chave);
    Task SalvarAsync(string chave, string valor, TimeSpan expiracao);
}
