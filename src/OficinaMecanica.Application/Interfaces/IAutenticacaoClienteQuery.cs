namespace OficinaMecanica.Application.Interfaces;

/// <summary>
/// Projeção de leitura somente-consulta usada pelo fluxo de autenticação por CPF.
/// Não é um repositório de agregado: existe para permitir que o use case de Auth
/// resolva se um CPF pertence a um cliente ativo sem depender de IClienteRepository
/// (ver regra de disciplina de agregados no CONTRIBUTING.md).
/// </summary>
public interface IAutenticacaoClienteQuery
{
    Task<DadosClienteAutenticacao?> ObterPorDocumentoAsync(string documento);
}

public record DadosClienteAutenticacao(Guid ClienteId, bool Ativo);
