namespace OficinaMecanica.Application.Interfaces;

public interface IIdempotencyStore
{
    Task<string?> ObterAsync(string chave);
    Task SalvarAsync(string chave, string valor, TimeSpan expiracao);

    /// <summary>
    /// Reserva a chave de forma atômica (SET NX). Retorna true se esta chamada
    /// obteve a reserva; false se outra requisição já a possui.
    /// </summary>
    Task<bool> TentarReservarAsync(string chave, TimeSpan expiracao);

    /// <summary>
    /// Remove a reserva/valor da chave. Usado para liberar a chave quando a
    /// requisição que a reservou falha ou não produz um resultado cacheável.
    /// </summary>
    Task RemoverAsync(string chave);
}
