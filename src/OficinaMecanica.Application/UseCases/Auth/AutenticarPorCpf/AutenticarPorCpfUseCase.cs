using OficinaMecanica.Application.Common;
using OficinaMecanica.Application.DTOs.Requests;
using OficinaMecanica.Application.DTOs.Responses;
using OficinaMecanica.Application.Interfaces;
using OficinaMecanica.Domain.Interfaces;
using OficinaMecanica.Domain.ValueObjects;

namespace OficinaMecanica.Application.UseCases.Auth.AutenticarPorCpf;

public class AutenticarPorCpfUseCase : IAutenticarPorCpfUseCase
{
    private readonly IAutenticacaoClienteQuery _autenticacaoClienteQuery;
    private readonly IUsuarioRepository _usuarioRepository;
    private readonly ITokenGenerator _tokenGenerator;

    public AutenticarPorCpfUseCase(
        IAutenticacaoClienteQuery autenticacaoClienteQuery,
        IUsuarioRepository usuarioRepository,
        ITokenGenerator tokenGenerator)
    {
        _autenticacaoClienteQuery = autenticacaoClienteQuery;
        _usuarioRepository = usuarioRepository;
        _tokenGenerator = tokenGenerator;
    }

    public async Task<Result<TokenResponse>> ExecutarAsync(AutenticarPorCpfRequest request)
    {
        Documento documento;
        try
        {
            documento = new Documento(request.Cpf);
        }
        catch (ArgumentException ex)
        {
            return Result<TokenResponse>.Validation(ex.Message);
        }

        var cliente = await _autenticacaoClienteQuery.ObterPorDocumentoAsync(documento.Valor);
        if (cliente is null || !cliente.Ativo)
            return Result<TokenResponse>.NotFound("Cliente não encontrado ou inativo.");

        var usuario = await _usuarioRepository.ObterPorClienteIdAsync(cliente.ClienteId);
        if (usuario is null)
            return Result<TokenResponse>.NotFound("Cliente não possui conta de acesso vinculada.");

        return Result<TokenResponse>.Success(_tokenGenerator.GerarParaUsuario(usuario));
    }
}
